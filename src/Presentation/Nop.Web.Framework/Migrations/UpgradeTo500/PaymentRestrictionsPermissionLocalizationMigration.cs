using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The advanced settings permission now also covers the payment method restrictions page; say so on the ACL page
/// </summary>
[NopUpdateMigration("2026-09-26 19:43:00", "5.00", UpdateMigrationType.Localization)]
public class PaymentRestrictionsPermissionLocalizationMigration : MigrationBase
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
            ["Security.Permission.Configuration.ManageAdvancedSettings"] = "Manage advanced settings (all settings, filter levels, GDPR, payment restrictions)"
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Security.Permission.Configuration.ManageAdvancedSettings"] = "إدارة الإعدادات المتقدمة (كل الإعدادات، إعدادات الفلترة، حماية البيانات GDPR، قيود الدفع)"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}