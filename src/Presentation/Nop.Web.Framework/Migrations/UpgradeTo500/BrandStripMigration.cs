using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-12 00:00:01", "5.00", UpdateMigrationType.Data)]
public class BrandStripMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();
        var settingService = EngineContext.Current.Resolve<ISettingService>();

        //the home page carries a brand strip under its own heading
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Homepage.Brands"] = "Brands"
        });

        //ponytail: match on the language prefix, not the exact culture - the pack ships ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Homepage.Brands"] = "العلامات التجارية"
            }, arabic.Id);

        //the strip is the whole block now, not a sidebar teaser: show every published brand
        var catalogSettings = settingService.LoadSetting<CatalogSettings>();
        catalogSettings.ManufacturersBlockItemsToDisplay = 8;
        settingService.SaveSetting(catalogSettings);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}