using FluentMigrator;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Security;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The activity log and activity types pages become super-administrator only
/// (and with them the activity log tab on the customer page, which checks the same permission)
/// </summary>
[NopUpdateMigration("2026-09-26 20:41:00", "5.00", UpdateMigrationType.Data)]
public class SuperAdministratorsActivityLogMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        SuperAdministratorsMigration.MoveToSuperAdministrators(EngineContext.Current.Resolve<INopDataProvider>(),
            StandardPermission.Customers.ACTIVITY_LOG_VIEW,
            StandardPermission.Customers.ACTIVITY_LOG_DELETE,
            StandardPermission.Customers.ACTIVITY_LOG_MANAGE_TYPES);

        //permission mappings were written past the services, so drop whatever start-up already cached
        EngineContext.Current.Resolve<IStaticCacheManager>().ClearAsync().Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}