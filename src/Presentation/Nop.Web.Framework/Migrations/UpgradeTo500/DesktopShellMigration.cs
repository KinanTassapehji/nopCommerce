using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The desktop shell: labels for the footer columns, and a parent category page lists the
/// products of its subcategories under the tiles instead of standing empty
/// </summary>
[NopUpdateMigration("2026-10-11 18:00:00", "5.00", UpdateMigrationType.Settings)]
public class DesktopShellMigration : MigrationBase
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

        var catalogSettings = settingService.LoadSetting<CatalogSettings>();
        catalogSettings.ShowProductsFromSubcategories = true;
        settingService.SaveSetting(catalogSettings);

        //English first, for every language; then Arabic for its own language only
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Footer.Shop"] = "Shop",
            ["Footer.Help"] = "Help",
            ["Footer.Tagline"] = "Everything your home needs, in one place. Fast delivery and cash on delivery."
        });

        //ponytail: match on the language prefix - the packs ship as ar-SY and ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Footer.Shop"] = "تسوق",
                ["Footer.Help"] = "المساعدة",
                ["Footer.Tagline"] = "كل ما يحتاجه بيتك في مكان واحد. توصيل سريع والدفع عند الاستلام."
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}