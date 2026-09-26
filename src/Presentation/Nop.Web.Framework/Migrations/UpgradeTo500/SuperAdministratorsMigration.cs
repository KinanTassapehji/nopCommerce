using FluentMigrator;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;
using Nop.Services.Security;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Splits "super administrators" out of "administrators": the System menu and the permissions (ACL) page
/// move to the new role, and the oldest active administrator (the one the installer created) joins it.
/// A super admin keeps the Administrators role too, so every other permission still reaches them.
/// </summary>
[NopUpdateMigration("2026-09-26 13:00:00", "5.00", UpdateMigrationType.Data)]
public class SuperAdministratorsMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var dataProvider = EngineContext.Current.Resolve<INopDataProvider>();
        var roles = dataProvider.GetTable<CustomerRole>();
        var administrators = roles.First(role => role.SystemName == NopCustomerDefaults.AdministratorsRoleName);

        //the permission config normally creates the role first (it runs before migrations), but not as a system role
        var superAdministrators = roles.FirstOrDefault(role => role.SystemName == NopCustomerDefaults.SuperAdministratorsRoleName)
            ?? dataProvider.InsertEntity(new CustomerRole { SystemName = NopCustomerDefaults.SuperAdministratorsRoleName });
        superAdministrators.Name = "Super Administrators";
        superAdministrators.Active = true;
        superAdministrators.IsSystemRole = true;
        dataProvider.UpdateEntity(superAdministrators);

        MoveToSuperAdministrators(dataProvider,
            StandardPermission.Security.MANAGE_PERMISSIONS,
            StandardPermission.System.MANAGE_SYSTEM_LOG,
            StandardPermission.System.MANAGE_MESSAGE_QUEUE,
            StandardPermission.System.MANAGE_MAINTENANCE,
            StandardPermission.System.MANAGE_SCHEDULE_TASKS,
            StandardPermission.System.MANAGE_APP_SETTINGS);

        var customerRoleMappings = dataProvider.GetTable<CustomerCustomerRoleMapping>();
        if (!customerRoleMappings.Any(mapping => mapping.CustomerRoleId == superAdministrators.Id))
        {
            var firstAdminId = (from mapping in customerRoleMappings
                                join customer in dataProvider.GetTable<Customer>() on mapping.CustomerId equals customer.Id
                                where mapping.CustomerRoleId == administrators.Id && customer.Active && !customer.Deleted
                                orderby customer.Id
                                select customer.Id).FirstOrDefault();
            if (firstAdminId > 0)
                dataProvider.InsertEntity(new CustomerCustomerRoleMapping { CustomerId = firstAdminId, CustomerRoleId = superAdministrators.Id });
        }

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.Customers.Customers.OnlySuperAdminCanManageSuperAdmin"] = "Only a super administrator can change super administrator accounts or the super administrators role.",
            ["Security.Permission.Security.ManagePermissions"] = "Manage permissions (access control list)"
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.Customers.Customers.OnlySuperAdminCanManageSuperAdmin"] = "لا يمكن تعديل حسابات المشرف العام أو دوره إلا من قبل مشرف عام.",
                ["Security.Permission.Security.ManagePermissions"] = "إدارة الصلاحيات (قائمة الصلاحيات)"
            }, arabic.Id);

        //roles and permission mappings were written past the services, so drop whatever start-up already cached
        EngineContext.Current.Resolve<IStaticCacheManager>().ClearAsync().Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }

    /// <summary>
    /// Takes the permissions off "Administrators" and grants them to "SuperAdministrators" only
    /// </summary>
    public static void MoveToSuperAdministrators(INopDataProvider dataProvider, params string[] permissionSystemNames)
    {
        var roles = dataProvider.GetTable<CustomerRole>();
        var administratorsId = roles.First(role => role.SystemName == NopCustomerDefaults.AdministratorsRoleName).Id;
        var superAdministratorsId = roles.First(role => role.SystemName == NopCustomerDefaults.SuperAdministratorsRoleName).Id;

        var permissionIds = dataProvider.GetTable<PermissionRecord>()
            .Where(permission => permissionSystemNames.Contains(permission.SystemName))
            .Select(permission => permission.Id)
            .ToList();
        var permissionMappings = dataProvider.GetTable<PermissionRecordCustomerRoleMapping>();
        foreach (var permissionId in permissionIds)
        {
            if (!permissionMappings.Any(mapping => mapping.PermissionRecordId == permissionId && mapping.CustomerRoleId == superAdministratorsId))
                dataProvider.InsertEntity(new PermissionRecordCustomerRoleMapping { PermissionRecordId = permissionId, CustomerRoleId = superAdministratorsId });

            foreach (var mapping in permissionMappings.Where(mapping => mapping.PermissionRecordId == permissionId && mapping.CustomerRoleId == administratorsId).ToList())
                dataProvider.DeleteEntity(mapping);
        }
    }
}