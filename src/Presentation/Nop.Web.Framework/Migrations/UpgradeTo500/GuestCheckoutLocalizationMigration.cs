using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// "Anonymous checkout" was machine-translated as anonymous log-out ("تسجيل الخروج"); it means buying as a guest.
/// Also names GDPR in the advanced settings permission, which now covers the GDPR settings page.
/// </summary>
[NopUpdateMigration("2026-09-26 18:30:00", "5.00", UpdateMigrationType.Localization)]
public class GuestCheckoutLocalizationMigration : MigrationBase
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
            ["Security.Permission.Configuration.ManageAdvancedSettings"] = "Manage advanced settings (all settings, filter levels, GDPR)"
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.Configuration.Settings.Order.AnonymousCheckoutAllowed"] = "السماح بالشراء كزائر",
                ["Admin.Configuration.Settings.Order.AnonymousCheckoutAllowed.Hint"] = "فعّل هذا الخيار للسماح بالشراء كزائر (لا يُطلب من العملاء تسجيل الدخول أو إنشاء حساب عند شراء المنتجات).",
                ["Security.Permission.Configuration.ManageAdvancedSettings"] = "إدارة الإعدادات المتقدمة (كل الإعدادات، إعدادات الفلترة، حماية البيانات GDPR)"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}