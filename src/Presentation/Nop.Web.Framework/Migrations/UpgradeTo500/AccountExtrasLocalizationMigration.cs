using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Account section extras: conditions of use, rate us, closing the account; the contact page's
/// channels and their admin settings
/// </summary>
//ponytail: migration only, not the language pack XMLs - both stores are installed; add them there if a fresh install is ever needed
[NopUpdateMigration("2026-10-04 16:47:00", "5.00", UpdateMigrationType.Localization)]
public class AccountExtrasLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //English first, for every language; then Arabic for its own language only - every key
        //is in both, so the English pass never leaves an Arabic label in English
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Account.ConditionsOfUse"] = "Conditions of use",
            ["Account.RateUs"] = "Rate us",
            ["Account.DeleteAccount"] = "Delete account",
            ["Account.DeleteAccount.Password"] = "Your password",
            ["Account.DeleteAccount.Warning"] = "Your account will be closed and you will be signed out, and you will not be able to sign in with it again. To continue, enter your password.",
            ["Account.DeleteAccount.Button"] = "Delete my account",
            ["Account.DeleteAccount.WrongPassword"] = "The password is incorrect.",
            ["Account.DeleteAccount.Done"] = "Your account has been deleted.",
            ["Account.DeleteAccount.AdminComment"] = "Account closed by the customer on {0} UTC (by the customer, not by an admin).",
            ["ContactUs.Channels.Phone"] = "Phone",
            ["ContactUs.Channels.Email"] = "Email us",
            ["ContactUs.Channels.WhatsApp.Hint"] = "Chat with us",
            ["ContactUs.Channels.Follow"] = "Follow us",
            ["Admin.Configuration.Settings.GeneralCommon.BlockTitle.SocialMedia"] = "Social media and contact",
            ["Admin.Configuration.Settings.GeneralCommon.ContactPhoneNumber"] = "Contact phone number",
            ["Admin.Configuration.Settings.GeneralCommon.ContactPhoneNumber.Hint"] = "The number customers call, shown on the contact us page (e.g. +963 11 123 4567). Leave empty to hide it.",
            ["Admin.Configuration.Settings.GeneralCommon.ContactEmail"] = "Contact email",
            ["Admin.Configuration.Settings.GeneralCommon.ContactEmail.Hint"] = "The address customers write to, shown on the contact us page. Leave empty to hide it.",
            ["Admin.Configuration.Settings.GeneralCommon.GooglePlayAppLink"] = "Google Play app URL",
            ["Admin.Configuration.Settings.GeneralCommon.GooglePlayAppLink.Hint"] = "The app's Google Play page, opened by \"Rate us\" in the account menu. Leave both app URLs empty to hide \"Rate us\".",
            ["Admin.Configuration.Settings.GeneralCommon.AppStoreAppLink"] = "App Store app URL",
            ["Admin.Configuration.Settings.GeneralCommon.AppStoreAppLink.Hint"] = "The app's App Store page, opened by \"Rate us\" on iPhone and iPad."
        });

        //ponytail: match on the language prefix - the packs ship as ar-SY and ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Account.ConditionsOfUse"] = "شروط الاستخدام",
                ["Account.RateUs"] = "قيّمنا",
                ["Account.DeleteAccount"] = "حذف الحساب",
                ["Account.DeleteAccount.Password"] = "كلمة المرور",
                ["Account.DeleteAccount.Warning"] = "سيتم إغلاق حسابك وتسجيل خروجك، ولن تتمكن من تسجيل الدخول به مرة أخرى. للمتابعة، أدخل كلمة المرور.",
                ["Account.DeleteAccount.Button"] = "حذف حسابي",
                ["Account.DeleteAccount.WrongPassword"] = "كلمة المرور غير صحيحة.",
                ["Account.DeleteAccount.Done"] = "تم حذف حسابك.",
                ["Account.DeleteAccount.AdminComment"] = "أغلق العميل حسابه بنفسه بتاريخ {0} بتوقيت UTC (من قبل العميل وليس من قبل الإدارة).",
                ["ContactUs.Channels.Phone"] = "الهاتف",
                ["ContactUs.Channels.Email"] = "راسلنا",
                ["ContactUs.Channels.WhatsApp.Hint"] = "تحدث معنا",
                ["ContactUs.Channels.Follow"] = "تابعنا",
                ["Admin.Configuration.Settings.GeneralCommon.BlockTitle.SocialMedia"] = "وسائل التواصل والاتصال",
                ["Admin.Configuration.Settings.GeneralCommon.ContactPhoneNumber"] = "رقم هاتف التواصل",
                ["Admin.Configuration.Settings.GeneralCommon.ContactPhoneNumber.Hint"] = "الرقم الذي يتصل به العملاء، ويظهر في صفحة اتصل بنا. اتركه فارغاً لإخفائه.",
                ["Admin.Configuration.Settings.GeneralCommon.ContactEmail"] = "البريد الإلكتروني للتواصل",
                ["Admin.Configuration.Settings.GeneralCommon.ContactEmail.Hint"] = "العنوان الذي يراسله العملاء، ويظهر في صفحة اتصل بنا. اتركه فارغاً لإخفائه.",
                ["Admin.Configuration.Settings.GeneralCommon.GooglePlayAppLink"] = "رابط التطبيق على Google Play",
                ["Admin.Configuration.Settings.GeneralCommon.GooglePlayAppLink.Hint"] = "صفحة التطبيق على Google Play، يفتحها زر \"قيّمنا\" في قائمة الحساب. اترك رابطي التطبيق فارغين لإخفاء الزر.",
                ["Admin.Configuration.Settings.GeneralCommon.AppStoreAppLink"] = "رابط التطبيق على App Store",
                ["Admin.Configuration.Settings.GeneralCommon.AppStoreAppLink.Hint"] = "صفحة التطبيق على App Store، يفتحها زر \"قيّمنا\" على أجهزة iPhone وiPad."
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}