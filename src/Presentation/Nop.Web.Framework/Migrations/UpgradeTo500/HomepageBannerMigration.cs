using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Moves the home page call to action from locale resources into HomepageBannerSettings, so the
/// admin edits it on its own page (Content management > Home page banner)
/// </summary>
[NopUpdateMigration("2026-10-05 12:00:00", "5.00", UpdateMigrationType.Settings)]
public class HomepageBannerMigration : MigrationBase
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

        //ponytail: match on the language prefix - the packs ship as ar-SY and ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));

        var settings = settingService.LoadSetting<HomepageBannerSettings>();
        if (!settingService.SettingExists(settings, s => s.Enabled))
        {
            //seeded with the copy the band showed before, in both languages
            settings.Enabled = true;
            settings.Title = "Everything your home needs, in one place";
            settings.Text = "Browse the full range and find your next favourite.";
            settings.ButtonText = "Shop now";
            settingService.SaveSetting(settings);

            if (arabic is not null)
            {
                localizationService.SaveLocalizedSettingAsync(settings, s => s.Title, arabic.Id, "كل ما يحتاجه منزلك في مكان واحد").Wait();
                localizationService.SaveLocalizedSettingAsync(settings, s => s.Text, arabic.Id, "تصفح المجموعة الكاملة واكتشف ما يناسبك.").Wait();
                localizationService.SaveLocalizedSettingAsync(settings, s => s.ButtonText, arabic.Id, "تسوق الآن").Wait();
            }
        }

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.ContentManagement.HomepageBanner"] = "Home page banner",
            ["Admin.ContentManagement.HomepageBanner.Fields.Enabled"] = "Show on home page",
            ["Admin.ContentManagement.HomepageBanner.Fields.Enabled.Hint"] = "Untick to hide the banner.",
            ["Admin.ContentManagement.HomepageBanner.Fields.Picture"] = "Cover picture",
            ["Admin.ContentManagement.HomepageBanner.Fields.Picture.Hint"] = "Wide image behind the text (about 1600x500). Leave empty for the plain brand colour.",
            ["Admin.ContentManagement.HomepageBanner.Fields.Title"] = "Title",
            ["Admin.ContentManagement.HomepageBanner.Fields.Title.Hint"] = "The heading of the banner.",
            ["Admin.ContentManagement.HomepageBanner.Fields.Text"] = "Text",
            ["Admin.ContentManagement.HomepageBanner.Fields.Text.Hint"] = "The line under the heading.",
            ["Admin.ContentManagement.HomepageBanner.Fields.ButtonText"] = "Button text",
            ["Admin.ContentManagement.HomepageBanner.Fields.ButtonText.Hint"] = "Leave empty to hide the button.",
            ["Admin.ContentManagement.HomepageBanner.Fields.ButtonUrl"] = "Button link",
            ["Admin.ContentManagement.HomepageBanner.Fields.ButtonUrl.Hint"] = "Where the button goes, e.g. /new-arrivals. Leave empty for the all-categories page."
        });

        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.ContentManagement.HomepageBanner"] = "بانر الصفحة الرئيسية",
                ["Admin.ContentManagement.HomepageBanner.Fields.Enabled"] = "عرض في الصفحة الرئيسية",
                ["Admin.ContentManagement.HomepageBanner.Fields.Enabled.Hint"] = "ألغِ التحديد لإخفاء البانر.",
                ["Admin.ContentManagement.HomepageBanner.Fields.Picture"] = "صورة الغلاف",
                ["Admin.ContentManagement.HomepageBanner.Fields.Picture.Hint"] = "صورة عريضة خلف النص (حوالي 1600×500). اتركها فارغة لاستخدام لون العلامة.",
                ["Admin.ContentManagement.HomepageBanner.Fields.Title"] = "العنوان",
                ["Admin.ContentManagement.HomepageBanner.Fields.Title.Hint"] = "عنوان البانر.",
                ["Admin.ContentManagement.HomepageBanner.Fields.Text"] = "النص",
                ["Admin.ContentManagement.HomepageBanner.Fields.Text.Hint"] = "السطر تحت العنوان.",
                ["Admin.ContentManagement.HomepageBanner.Fields.ButtonText"] = "نص الزر",
                ["Admin.ContentManagement.HomepageBanner.Fields.ButtonText.Hint"] = "اتركه فارغاً لإخفاء الزر.",
                ["Admin.ContentManagement.HomepageBanner.Fields.ButtonUrl"] = "رابط الزر",
                ["Admin.ContentManagement.HomepageBanner.Fields.ButtonUrl.Hint"] = "وجهة الزر، مثل /new-arrivals. اتركه فارغاً لصفحة جميع الفئات."
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}