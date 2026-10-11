using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The greeting in the phone home page header, the rotating search hint, and the account profile card
/// </summary>
[NopUpdateMigration("2026-10-11 12:00:03", "5.00", UpdateMigrationType.Localization)]
public class HomeHeaderLocalizationMigration : MigrationBase
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
            ["Mobile.Home.Hello"] = "Hello",
            ["Mobile.Home.HelloName"] = "Hello, {0}",
            ["Mobile.Home.Tagline"] = "What are you looking for today?",
            ["Search.SearchBox.Example"] = "Search for {0}",
            ["Account.Profile.Guest"] = "Welcome",
            ["Account.Profile.SignIn"] = "Sign in or create an account"
        });

        //ponytail: match on the language prefix - the packs ship as ar-SY and ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Mobile.Home.Hello"] = "أهلاً بك",
                ["Mobile.Home.HelloName"] = "أهلاً، {0}",
                ["Mobile.Home.Tagline"] = "ماذا تبحث عنه اليوم؟",
                ["Search.SearchBox.Example"] = "ابحث عن {0}",
                ["Account.Profile.Guest"] = "أهلاً بك",
                ["Account.Profile.SignIn"] = "سجّل دخولك أو أنشئ حساباً"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}