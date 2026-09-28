using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The Arabic language pack machine-translated "GDPR" as "gross national/domestic product"
/// </summary>
[NopUpdateMigration("2026-09-26 18:00:00", "5.00", UpdateMigrationType.Localization)]
public class GdprSettingsLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        foreach (var arabic in languageService.GetAllLanguages(showHidden: true)
            .Where(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase)))
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.Configuration.Settings.Gdpr"] = "إعدادات حماية البيانات (GDPR)",
                ["Admin.Configuration.Settings.Gdpr.GdprEnabled.Hint"] = "حدّد لتمكين اللائحة العامة لحماية البيانات (GDPR).",
                ["Admin.Configuration.Settings.Gdpr.GdprEnabled"] = "تمكين حماية البيانات (GDPR)",
                ["Admin.Configuration.Settings.Gdpr.Consent.Updated"] = "تم تحديث موافقة حماية البيانات (GDPR) بنجاح.",
                ["Admin.Customers.Customers.Gdpr"] = "حماية البيانات (GDPR)",
                ["Account.Gdpr"] = "أدوات حماية البيانات (GDPR)"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
