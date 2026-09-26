using FluentMigrator;
using Nop.Core.Caching;
using Nop.Core.Domain.Configuration;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The stores sell to registered customers only: guest checkout goes off (the installer had it on), which also
/// retires the "checkout as guest or register" box on the login page
/// </summary>
[NopUpdateMigration("2026-09-26 22:29:00", "5.00", UpdateMigrationType.Settings)]
public class GuestCheckoutDisabledMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var dataProvider = EngineContext.Current.Resolve<INopDataProvider>();

        //the shared value and any per-store override
        foreach (var setting in dataProvider.GetTable<Setting>().Where(s => s.Name == "ordersettings.anonymouscheckoutallowed").ToList())
        {
            setting.Value = false.ToString();
            dataProvider.UpdateEntity(setting);
        }

        //the setting was written past the setting service, so drop whatever start-up already cached
        EngineContext.Current.Resolve<IStaticCacheManager>().ClearAsync().Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}