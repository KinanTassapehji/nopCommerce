using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The permissions card on the customer role page
/// </summary>
[NopUpdateMigration("2026-09-29 23:50:00", "5.00", UpdateMigrationType.Localization)]
public class RolePermissionsLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.Customers.CustomerRoles.Permissions"] = "Permissions",
            ["Admin.Customers.CustomerRoles.Permissions.Hint"] = "Tick what people in this role may do; hover over a box to see what it allows. To let them into the admin area at all, tick \"Access admin area\" under Security. You can only grant permissions you have yourself.",
            ["Admin.Customers.CustomerRoles.Permissions.All"] = "All",
            ["Admin.Customers.CustomerRoles.Permissions.Search"] = "Search permissions",
            ["Admin.Customers.CustomerRoles.Permissions.Selected"] = "{0} of {1} allowed",
            ["Admin.Customers.CustomerRoles.Permissions.NoMatch"] = "No permissions match your search.",
            ["Admin.Customers.CustomerRoles.Permissions.Category.Security"] = "Security",
            ["Admin.Customers.CustomerRoles.Permissions.Category.Orders"] = "Orders",
            ["Admin.Customers.CustomerRoles.Permissions.Category.Catalog"] = "Catalog",
            ["Admin.Customers.CustomerRoles.Permissions.Category.Customers"] = "Customers",
            ["Admin.Customers.CustomerRoles.Permissions.Category.Promotions"] = "Promotions",
            ["Admin.Customers.CustomerRoles.Permissions.Category.ContentManagement"] = "Content management",
            ["Admin.Customers.CustomerRoles.Permissions.Category.Reports"] = "Reports",
            ["Admin.Customers.CustomerRoles.Permissions.Category.Configuration"] = "Configuration",
            ["Admin.Customers.CustomerRoles.Permissions.Category.System"] = "System",
            ["Admin.Customers.CustomerRoles.Permissions.Category.PublicStore"] = "Public store"
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.Customers.CustomerRoles.Permissions"] = "الصلاحيات",
                ["Admin.Customers.CustomerRoles.Permissions.Hint"] = "حدّد ما يمكن لأصحاب هذا الدور فعله؛ مرّر المؤشر فوق أي خيار لمعرفة ما يسمح به. للسماح لهم بدخول لوحة الإدارة أصلاً، حدّد \"الدخول للوحة التحكم\" ضمن الأمان. لا يمكنك منح صلاحيات لا تملكها أنت.",
                ["Admin.Customers.CustomerRoles.Permissions.All"] = "الكل",
                ["Admin.Customers.CustomerRoles.Permissions.Search"] = "ابحث في الصلاحيات",
                ["Admin.Customers.CustomerRoles.Permissions.Selected"] = "{0} من {1} مسموحة",
                ["Admin.Customers.CustomerRoles.Permissions.NoMatch"] = "لا توجد صلاحيات مطابقة للبحث.",
                ["Admin.Customers.CustomerRoles.Permissions.Category.Security"] = "الأمان",
                ["Admin.Customers.CustomerRoles.Permissions.Category.Orders"] = "الطلبات",
                ["Admin.Customers.CustomerRoles.Permissions.Category.Catalog"] = "الكتالوج",
                ["Admin.Customers.CustomerRoles.Permissions.Category.Customers"] = "العملاء",
                ["Admin.Customers.CustomerRoles.Permissions.Category.Promotions"] = "العروض والتسويق",
                ["Admin.Customers.CustomerRoles.Permissions.Category.ContentManagement"] = "إدارة المحتوى",
                ["Admin.Customers.CustomerRoles.Permissions.Category.Reports"] = "التقارير",
                ["Admin.Customers.CustomerRoles.Permissions.Category.Configuration"] = "الإعدادات",
                ["Admin.Customers.CustomerRoles.Permissions.Category.System"] = "النظام",
                ["Admin.Customers.CustomerRoles.Permissions.Category.PublicStore"] = "المتجر"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}