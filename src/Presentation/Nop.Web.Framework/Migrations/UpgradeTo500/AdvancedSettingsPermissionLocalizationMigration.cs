using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Name of the super-administrator-only "advanced settings" permission on the ACL page;
/// the permission itself is installed at start-up from the permission config
/// </summary>
[NopUpdateMigration("2026-09-26 17:00:00", "5.00", UpdateMigrationType.Localization)]
public class AdvancedSettingsPermissionLocalizationMigration : MigrationBase
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
            ["Security.Permission.Configuration.ManageAdvancedSettings"] = "Manage advanced settings (all settings, filter levels)"
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Security.Permission.Configuration.ManageAdvancedSettings"] = "إدارة الإعدادات المتقدمة (كل الإعدادات، إعدادات الفلترة)"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}