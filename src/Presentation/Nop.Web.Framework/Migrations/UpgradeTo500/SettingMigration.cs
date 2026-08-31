using FluentMigrator;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-08-23 00:00:00", "5.00", UpdateMigrationType.Settings)]
public class SettingMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();

        //TmTm does not charge tax - keep every tax surface out of the UI.
        //ponytail: settings, not view surgery - every tax row/label/selector in the store is already gated by these.
        var taxSettings = settingService.LoadSetting<TaxSettings>();
        taxSettings.ActiveTaxProviderSystemName = string.Empty;
        taxSettings.TaxDisplayType = TaxDisplayType.ExcludingTax;
        taxSettings.AllowCustomersToSelectTaxDisplayType = false;
        taxSettings.DisplayTaxSuffix = false;
        taxSettings.DisplayTaxRates = false;
        taxSettings.HideZeroTax = true;
        taxSettings.HideTaxInOrderSummary = true;
        taxSettings.ShippingIsTaxable = false;
        taxSettings.PaymentMethodAdditionalFeeIsTaxable = false;
        taxSettings.EuVatEnabled = false;
        settingService.SaveSetting(taxSettings);

        var catalogSettings = settingService.LoadSetting<Core.Domain.Catalog.CatalogSettings>();
        catalogSettings.DisplayTaxShippingInfoFooter = false;
        catalogSettings.DisplayTaxShippingInfoProductDetailsPage = false;
        catalogSettings.DisplayTaxShippingInfoProductBoxes = false;
        catalogSettings.DisplayTaxShippingInfoShoppingCart = false;

        catalogSettings.DisplayTaxShippingInfoOrderDetailsPage = false;
        settingService.SaveSetting(catalogSettings);

        //product specifications are gone; drop the rows that used to configure them
        var obsolete = new[]
        {
            "catalogsettings.enablespecificationattributefiltering",
            "catalogsettings.exportimportproductspecificationattributes",
            "producteditorsettings.specificationattributes"
        };

        foreach (var setting in settingService.GetAllSettings().Where(setting => obsolete.Contains(setting.Name)).ToList())
            settingService.DeleteSetting(setting);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}