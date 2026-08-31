using FluentMigrator;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-08-31 00:00:00", "5.00", UpdateMigrationType.Settings)]
public class ShipToPopUpMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //the "ship to" popup asks for a city, an area and a street - Syrian addresses have no postal code
        var shippingSettings = settingService.LoadSetting<ShippingSettings>();
        shippingSettings.EstimateShippingCityNameEnabled = true;
        settingService.SaveSetting(shippingSettings);

        //the country picker is fixed to Syria and hidden, so the remaining fields move one step down:
        //the state/province list holds the governorates (the "city"), and the free text field is the area
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Shipping.EstimateShippingPopUp.StateProvince"] = "City",
            ["Shipping.EstimateShippingPopUp.City"] = "Area",
            ["Shipping.EstimateShippingPopUp.Street"] = "Street",
            ["Shipping.EstimateShipping.City.Required"] = "Area is required"
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Shipping.EstimateShippingPopUp.ShipToTitle"] = "شحن إلى",
                ["Shipping.EstimateShippingPopUp.StateProvince"] = "المدينة",
                ["Shipping.EstimateShippingPopUp.City"] = "المنطقة",
                ["Shipping.EstimateShippingPopUp.Street"] = "الشارع",
                ["Shipping.EstimateShipping.City.Required"] = "المنطقة مطلوبة"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}