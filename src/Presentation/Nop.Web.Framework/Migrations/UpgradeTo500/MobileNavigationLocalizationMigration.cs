using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Labels for the mobile tab bar. The stock resources are sentences —
/// "Home page", "Shopping cart" — and a tab label has room for one word, so
/// these two are the only new keys the mobile shell needs; the rest of the bar
/// reuses "Categories" and "Account.MyAccount", which are already short.
/// </summary>
[NopUpdateMigration("2026-08-28 00:00:01", "5.00", UpdateMigrationType.Localization)]
public class MobileNavigationLocalizationMigration : MigrationBase
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
            ["Mobile.Nav.Home"] = "Home",
            ["Mobile.Nav.Cart"] = "Cart"
        });

        //the store ships ar-SY, but match on the language rather than the
        //region so a store installed with any other Arabic culture is covered
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Mobile.Nav.Home"] = "الرئيسية",
                ["Mobile.Nav.Cart"] = "السلة"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}