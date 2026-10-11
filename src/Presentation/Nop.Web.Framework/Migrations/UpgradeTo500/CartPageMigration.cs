using FluentMigrator;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The cart page loses the "estimate shipping" button: delivery is one flat step the checkout
/// states, and the button opened a form nobody needed to fill
/// </summary>
[NopUpdateMigration("2026-10-11 18:30:00", "5.00", UpdateMigrationType.Settings)]
public class CartPageMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var shippingSettings = settingService.LoadSetting<ShippingSettings>();
        shippingSettings.EstimateShippingCartPageEnabled = false;
        settingService.SaveSetting(shippingSettings);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}