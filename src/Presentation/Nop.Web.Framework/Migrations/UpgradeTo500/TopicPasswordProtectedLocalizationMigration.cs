using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The pages list's "password protected" column read "كلمة السر المحمية" ("the protected
/// password"), and its hint called the page a "topic" (الموضوع), unlike the rest of the admin
/// </summary>
[NopUpdateMigration("2026-10-03 19:30:00", "5.00", UpdateMigrationType.Localization)]
public class TopicPasswordProtectedLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //Arabic only: English already reads "Password protected"
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is null)
            return;

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.ContentManagement.Topics.Fields.IsPasswordProtected"] = "محمية بكلمة مرور",
            ["Admin.ContentManagement.Topics.Fields.IsPasswordProtected.Hint"] = "حدّد لحماية هذه الصفحة بكلمة مرور."
        }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
