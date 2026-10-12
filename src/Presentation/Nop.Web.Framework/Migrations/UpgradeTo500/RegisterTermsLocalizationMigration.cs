using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The register page's conditions of use checkbox and its modal
/// </summary>
[NopUpdateMigration("2026-10-04 10:00:00", "5.00", UpdateMigrationType.Localization)]
public class RegisterTermsLocalizationMigration : MigrationBase
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
            ["Account.Fields.AcceptTerms"] = "I agree to the",
            ["Account.Fields.AcceptTerms.Link"] = "Conditions of use",
            ["Account.Fields.AcceptTerms.Required"] = "Please agree to the conditions of use to create your account.",
            ["Account.Fields.AcceptTerms.Agree"] = "I agree"
        });

        //ponytail: match on the language prefix - the packs ship as ar-SY and ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Account.Fields.AcceptTerms"] = "أوافق على",
                ["Account.Fields.AcceptTerms.Link"] = "شروط الاستخدام",
                ["Account.Fields.AcceptTerms.Required"] = "يرجى الموافقة على شروط الاستخدام لإنشاء حسابك.",
                ["Account.Fields.AcceptTerms.Agree"] = "أوافق على الشروط"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
