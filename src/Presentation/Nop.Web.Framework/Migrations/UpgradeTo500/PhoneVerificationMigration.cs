using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Phone numbers are confirmed with a code sent over WhatsApp, from the store's own number
/// through the sidecar in deploy/whatsapp-sidecar. It starts switched off: a super administrator
/// links the number (Configuration > Settings > Phone verification) and then turns it on.
/// </summary>
[NopUpdateMigration("2026-10-04 12:00:00", "5.00", UpdateMigrationType.Settings)]
public class PhoneVerificationMigration : MigrationBase
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

        if (!settingService.SettingExists(new PhoneVerificationSettings(), settings => settings.WhatsAppSidecarUrl))
        {
            settingService.SaveSetting(new PhoneVerificationSettings
            {
                Enabled = false,
                //the port deploy/whatsapp-sidecar/systemd/tmtm-whatsapp.service listens on
                WhatsAppSidecarUrl = "http://127.0.0.1:3210"
            });
        }

        foreach (var language in languageService.GetAllLanguages(showHidden: true))
        {
            var arabic = language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                //the verify page
                ["Account.PhoneVerification"] = arabic ? "تأكيد رقم الهاتف" : "Confirm your phone number",
                ["Account.PhoneVerification.Sent"] = arabic ? "أرسلنا رمز تحقق عبر واتساب إلى {0}. أدخله هنا." : "We sent a code over WhatsApp to {0}. Enter it here.",
                ["Account.PhoneVerification.Code"] = arabic ? "الرمز" : "Code",
                ["Account.PhoneVerification.Verify"] = arabic ? "تأكيد" : "Confirm",
                ["Account.PhoneVerification.Resend"] = arabic ? "إرسال رمز جديد" : "Send a new code",
                ["Account.PhoneVerification.PhoneChanged"] = arabic ? "تم تغيير رقم هاتفك." : "Your phone number is changed.",
                //the WhatsApp message itself
                ["Account.PhoneVerification.Message"] = arabic
                    ? "رمز التحقق في تمتم: {0}\nصالح لمدة 10 دقائق. لا تشاركه مع أحد."
                    : "Your TmTm code: {0}\nValid for 10 minutes. Don't share it with anyone.",
                //sending
                ["Account.PhoneVerification.Send.Sent"] = arabic ? "أرسلنا رمزاً جديداً." : "A new code is on its way.",
                ["Account.PhoneVerification.Send.TooSoon"] = arabic ? "أُرسل رمز للتو. انتظر دقيقة قبل طلب رمز آخر." : "A code was just sent. Wait a minute before asking for another.",
                ["Account.PhoneVerification.Send.TooMany"] = arabic ? "طلبت رموزاً كثيرة. حاول مجدداً بعد ساعة." : "Too many codes asked for. Try again in an hour.",
                ["Account.PhoneVerification.Send.NotOnWhatsApp"] = arabic ? "هذا الرقم غير مسجّل في واتساب. استخدم رقماً عليه واتساب." : "This number isn't on WhatsApp. Use a number that has WhatsApp.",
                ["Account.PhoneVerification.Send.Failed"] = arabic ? "تعذّر إرسال الرمز الآن. حاول مجدداً بعد دقائق." : "The code couldn't be sent right now. Try again in a few minutes.",
                //checking
                ["Account.PhoneVerification.Check.Wrong"] = arabic ? "الرمز غير صحيح." : "That code isn't right.",
                ["Account.PhoneVerification.Check.Expired"] = arabic ? "انتهت صلاحية الرمز. اطلب رمزاً جديداً." : "The code has expired. Ask for a new one.",
                ["Account.PhoneVerification.Check.TooManyAttempts"] = arabic ? "أدخلت رموزاً خاطئة كثيرة. اطلب رمزاً جديداً." : "Too many wrong codes. Ask for a new one.",
                ["Account.PhoneVerification.Check.NoCode"] = arabic ? "هذا الرمز مستخدم أو لم يُرسل. اطلب رمزاً جديداً." : "This code was used already, or none was sent. Ask for a new one.",
                //password recovery by phone
                ["Account.PasswordRecovery.Phone"] = arabic ? "رقم الهاتف" : "Phone number",
                ["Account.PasswordRecovery.PhoneTooltip"] = arabic ? "أدخل رقم هاتفك وسنرسل لك رمزاً عبر واتساب لتعيين كلمة مرور جديدة." : "Enter your phone number and we'll send you a code over WhatsApp to set a new password.",
                ["Account.PasswordRecovery.OrByEmail"] = arabic ? "أو، إن كان لحسابك بريد إلكتروني، أدخله لنرسل لك رابطاً:" : "Or, if your account has an email, enter it and we'll send you a link:",
                ["Account.PasswordRecovery.PhoneNotFound"] = arabic ? "لا يوجد حساب بهذا الرقم." : "No account has this phone number.",
                //admin
                ["Admin.Configuration.Settings.PhoneVerification"] = arabic ? "التحقق من رقم الهاتف" : "Phone verification",
                ["Admin.Configuration.Settings.PhoneVerification.Enabled"] = arabic ? "طلب رمز تحقق" : "Require a code",
                ["Admin.Configuration.Settings.PhoneVerification.Enabled.Hint"] = arabic
                    ? "الحسابات الجديدة، وتغيير رقم الهاتف، واستعادة كلمة المرور بالهاتف تحتاج رمزاً يُرسل عبر واتساب. فعّله بعد ربط رقم المتجر أدناه."
                    : "New accounts, a changed phone number and password recovery by phone need a code sent over WhatsApp. Turn it on once the store's number below is linked.",
                ["Admin.Configuration.Settings.PhoneVerification.WhatsAppSidecarUrl"] = arabic ? "عنوان خدمة واتساب" : "WhatsApp sidecar address",
                ["Admin.Configuration.Settings.PhoneVerification.WhatsAppSidecarUrl.Hint"] = arabic
                    ? "عنوان خدمة واتساب على الخادم (tmtm-whatsapp)، عادةً http://127.0.0.1:3210."
                    : "Address of the WhatsApp sidecar on the server (tmtm-whatsapp), normally http://127.0.0.1:3210.",
                ["Admin.Configuration.Settings.PhoneVerification.LogMode"] = arabic
                    ? "بدون عنوان: تُكتب الرموز في سجل النظام بدل إرسالها (للتجربة فقط)."
                    : "No address: codes are written to the system log instead of sent (for testing only).",
                ["Admin.Configuration.Settings.PhoneVerification.NoNumberWarning"] = arabic
                    ? "التحقق مفعّل لكن لا يوجد رقم واتساب متصل: لن يتمكن أحد من إنشاء حساب أو استعادة كلمة المرور بالهاتف."
                    : "Verification is on but no WhatsApp number is connected: nobody can register or recover a password by phone.",
                ["Admin.Configuration.Settings.PhoneVerification.Numbers"] = arabic ? "أرقام واتساب المتجر" : "Store WhatsApp numbers",
                ["Admin.Configuration.Settings.PhoneVerification.NoNumbers"] = arabic ? "لم يُربط أي رقم بعد." : "No number linked yet.",
                ["Admin.Configuration.Settings.PhoneVerification.AddNumber"] = arabic ? "ربط رقم" : "Link a number",
                ["Admin.Configuration.Settings.PhoneVerification.Unlink"] = arabic ? "فك الربط" : "Unlink",
                ["Admin.Configuration.Settings.PhoneVerification.ScanQr"] = arabic
                    ? "على هاتف المتجر: واتساب ← الإعدادات ← الأجهزة المرتبطة ← ربط جهاز، ثم امسح هذا الرمز."
                    : "On the store's phone: WhatsApp > Settings > Linked devices > Link a device, then scan this code.",
                ["Admin.Configuration.Settings.PhoneVerification.SidecarOffline"] = arabic
                    ? "خدمة واتساب لا تستجيب على هذا العنوان. تحقق من أنها تعمل على الخادم (systemctl status tmtm-whatsapp)."
                    : "The WhatsApp sidecar does not answer at this address. Check it runs on the server (systemctl status tmtm-whatsapp).",
                ["Admin.Configuration.Settings.PhoneVerification.State.starting"] = arabic ? "قيد التشغيل…" : "Starting…",
                ["Admin.Configuration.Settings.PhoneVerification.State.qr"] = arabic ? "بانتظار مسح الرمز" : "Waiting for the QR scan",
                ["Admin.Configuration.Settings.PhoneVerification.State.authenticated"] = arabic ? "تم المسح، جارٍ الاتصال…" : "Scanned, connecting…",
                ["Admin.Configuration.Settings.PhoneVerification.State.ready"] = arabic ? "متصل ويرسل الرموز" : "Connected, sending codes",
                ["Admin.Configuration.Settings.PhoneVerification.State.disconnected"] = arabic ? "غير متصل" : "Disconnected"
            }, language.Id);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}