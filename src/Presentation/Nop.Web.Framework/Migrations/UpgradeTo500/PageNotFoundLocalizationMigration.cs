using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The ways out of the "page not found" page: search, home, categories, recently viewed
/// </summary>
[NopUpdateMigration("2026-10-03 19:00:00", "5.00", UpdateMigrationType.Localization)]
public class PageNotFoundLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //English first, for every language; then Arabic for its own language only
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["PageNotFound.Search.Placeholder"] = "Search for a product...",
            ["PageNotFound.Search.Button"] = "Search",
            ["PageNotFound.GoHome"] = "Back to home",
            ["PageNotFound.BrowseCategories"] = "Browse categories",
            ["PageNotFound.RecentlyViewed"] = "Recently viewed"
        });

        //ponytail: match on the language prefix - the packs ship as ar-SY and ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["PageNotFound.Search.Placeholder"] = "ابحث عن منتج...",
                ["PageNotFound.Search.Button"] = "بحث",
                ["PageNotFound.GoHome"] = "العودة إلى الرئيسية",
                ["PageNotFound.BrowseCategories"] = "تصفّح الأقسام",
                ["PageNotFound.RecentlyViewed"] = "تصفّحته مؤخرًا"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
