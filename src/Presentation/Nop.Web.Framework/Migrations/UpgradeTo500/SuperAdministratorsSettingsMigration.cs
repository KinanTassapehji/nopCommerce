using FluentMigrator;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Security;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The whole Configuration > Settings group becomes super-administrator only. "Manage settings" also covers the
/// pages reached from inside it (customer/address/vendor attributes, return request reasons and actions,
/// review types) and the product editor settings button.
/// </summary>
[NopUpdateMigration("2026-09-26 19:07:00", "5.00", UpdateMigrationType.Data)]
public class SuperAdministratorsSettingsMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        SuperAdministratorsMigration.MoveToSuperAdministrators(EngineContext.Current.Resolve<INopDataProvider>(),
            StandardPermission.Configuration.MANAGE_SETTINGS);

        //permission mappings were written past the services, so drop whatever start-up already cached
        EngineContext.Current.Resolve<IStaticCacheManager>().ClearAsync().Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}