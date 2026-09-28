using FluentMigrator;
using Nop.Core.Caching;
using Nop.Core.Domain.Configuration;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Cash on delivery: a real checkout description in English and Arabic instead of the plugin's sample text
/// ("...P.S. You can edit this text from admin panel"), and Arabic labels on its configuration page.
/// A description someone already wrote is left alone: only the sample text (either language) is replaced.
/// </summary>
[NopUpdateMigration("2026-09-26 21:13:00", "5.00", UpdateMigrationType.Localization)]
public class CashOnDeliveryTextMigration : MigrationBase
{
    private const string ENGLISH_DESCRIPTION = "<p>Pay in cash when your order is delivered.</p>"
        + "<p>Once you place your order, one of our team will call you to confirm it and arrange the delivery time. We start preparing your order as soon as it is confirmed.</p>"
        + "<p>Please note that confirmed orders cannot be cancelled.</p>";

    private const string ARABIC_DESCRIPTION = "<p>ادفع نقداً عند استلام طلبك.</p>"
        + "<p>بعد إتمام الطلب، سيتواصل معك أحد أفراد فريقنا هاتفياً لتأكيد الطلب وتحديد موعد التوصيل، ونبدأ بتجهيز طلبك فور تأكيده.</p>"
        + "<p>يُرجى العلم أنه لا يمكن إلغاء الطلب بعد تأكيده.</p>";

    //the English sample, and the Arabic translations of it the installer has shipped
    private static bool IsSampleText(string value) => string.IsNullOrWhiteSpace(value)
        || value.Contains("authorized representative", StringComparison.OrdinalIgnoreCase)
        || value.Contains("يمكنكم تعديل هذا النص");

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var dataProvider = EngineContext.Current.Resolve<INopDataProvider>();
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var localizedEntityService = EngineContext.Current.Resolve<ILocalizedEntityService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        var languages = languageService.GetAllLanguages(showHidden: true);
        var arabic = languages.FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));

        //the description is shared by all stores (store 0); a per-store override is somebody's own text
        var setting = dataProvider.GetTable<Setting>()
            .FirstOrDefault(s => s.Name == "cashondeliverypaymentsettings.descriptiontext" && s.StoreId == 0);
        if (setting is not null)
        {
            if (IsSampleText(setting.Value))
            {
                setting.Value = arabic is not null ? ARABIC_DESCRIPTION : ENGLISH_DESCRIPTION;
                dataProvider.UpdateEntity(setting);
            }

            foreach (var language in languages)
            {
                var localized = localizationService.GetLocalizedAsync(setting, x => x.Value, language.Id, false, false).GetAwaiter().GetResult();
                if (!IsSampleText(localized))
                    continue;

                var text = language.Id == arabic?.Id ? ARABIC_DESCRIPTION : ENGLISH_DESCRIPTION;
                localizedEntityService.SaveLocalizedValueAsync(setting, x => x.Value, text, language.Id).GetAwaiter().GetResult();
            }
        }

        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.Payment.CashOnDelivery.DescriptionText"] = "الوصف",
                ["Plugins.Payment.CashOnDelivery.DescriptionText.Hint"] = "النص الذي يظهر للعملاء عند اختيار الدفع عند الاستلام أثناء إتمام الطلب.",
                ["Plugins.Payment.CashOnDelivery.AdditionalFee"] = "رسوم إضافية",
                ["Plugins.Payment.CashOnDelivery.AdditionalFee.Hint"] = "رسوم تُضاف إلى الطلب عند اختيار الدفع عند الاستلام.",
                ["Plugins.Payment.CashOnDelivery.AdditionalFeePercentage"] = "الرسوم الإضافية كنسبة مئوية",
                ["Plugins.Payment.CashOnDelivery.AdditionalFeePercentage.Hint"] = "حدّد لاحتساب الرسوم الإضافية كنسبة مئوية من إجمالي الطلب. إذا لم تُحدَّد، تُستخدم قيمة ثابتة.",
                ["Plugins.Payment.CashOnDelivery.ShippableProductRequired"] = "يتطلب منتجاً قابلاً للشحن",
                ["Plugins.Payment.CashOnDelivery.ShippableProductRequired.Hint"] = "حدّد لإظهار الدفع عند الاستلام فقط عندما تحتوي السلة على منتجات قابلة للشحن.",
                ["Plugins.Payment.CashOnDelivery.SkipPaymentInfo"] = "تخطي صفحة معلومات الدفع",
                ["Plugins.Payment.CashOnDelivery.SkipPaymentInfo.Hint"] = "حدّد لتخطي صفحة معلومات الدفع لهذه الطريقة أثناء إتمام الطلب."
            }, arabic.Id);

        //the setting was written past the setting service, so drop whatever start-up already cached
        EngineContext.Current.Resolve<IStaticCacheManager>().ClearAsync().Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}