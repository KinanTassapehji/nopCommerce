using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Labels of Content management > Home page features, and the names of the icons it offers
/// </summary>
[NopUpdateMigration("2026-10-03 18:00:02", "5.00", UpdateMigrationType.Localization)]
public class HomepageFeatureLocalizationMigration : MigrationBase
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
            ["Admin.ContentManagement.HomepageFeatures"] = "Home page features",
            ["Admin.ContentManagement.HomepageFeatures.Hint"] = "The short reasons to buy shown in a strip under the home page slider (fast delivery, cash on delivery...). Unpublish an item to hide it without deleting it.",
            ["Admin.ContentManagement.HomepageFeatures.AddNew"] = "Add a new home page feature",
            ["Admin.ContentManagement.HomepageFeatures.EditDetails"] = "Edit home page feature",
            ["Admin.ContentManagement.HomepageFeatures.BackToList"] = "back to home page features",
            ["Admin.ContentManagement.HomepageFeatures.Added"] = "The home page feature has been added successfully.",
            ["Admin.ContentManagement.HomepageFeatures.Updated"] = "The home page feature has been updated successfully.",
            ["Admin.ContentManagement.HomepageFeatures.Deleted"] = "The home page feature has been deleted successfully.",
            ["Admin.ContentManagement.HomepageFeatures.Fields.Title"] = "Title",
            ["Admin.ContentManagement.HomepageFeatures.Fields.Title.Hint"] = "The bold line, e.g. \"Fast delivery\". Keep it to two or three words.",
            ["Admin.ContentManagement.HomepageFeatures.Fields.Title.Required"] = "Please enter a title.",
            ["Admin.ContentManagement.HomepageFeatures.Fields.Hint"] = "Sub-line",
            ["Admin.ContentManagement.HomepageFeatures.Fields.Hint.Hint"] = "The short line under the title, e.g. \"Straight to your door\". Optional.",
            ["Admin.ContentManagement.HomepageFeatures.Fields.Icon"] = "Icon",
            ["Admin.ContentManagement.HomepageFeatures.Fields.Icon.Hint"] = "The picture beside the text.",
            ["Admin.ContentManagement.HomepageFeatures.Fields.Published"] = "Published",
            ["Admin.ContentManagement.HomepageFeatures.Fields.Published.Hint"] = "Show this item on the home page.",
            ["Admin.ContentManagement.HomepageFeatures.Fields.DisplayOrder"] = "Display order",
            ["Admin.ContentManagement.HomepageFeatures.Fields.DisplayOrder.Hint"] = "Items are shown from the lowest number to the highest.",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Delivery"] = "Delivery",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.CashOnDelivery"] = "Cash",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Genuine"] = "Genuine",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Support"] = "Customer care",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Returns"] = "Returns",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.SecurePayment"] = "Secure payment",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Gift"] = "Gift",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Offers"] = "Offers",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Clock"] = "Clock",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Star"] = "Star",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Phone"] = "Phone",
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Store"] = "Store"
        });

        //ponytail: match on the language prefix - the packs ship as ar-SY and ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.ContentManagement.HomepageFeatures"] = "مميزات الصفحة الرئيسية",
                ["Admin.ContentManagement.HomepageFeatures.Hint"] = "أسباب الشراء القصيرة التي تظهر في شريط تحت عارض الصور في الصفحة الرئيسية (توصيل سريع، الدفع عند الاستلام...). ألغِ نشر عنصر لإخفائه دون حذفه.",
                ["Admin.ContentManagement.HomepageFeatures.AddNew"] = "إضافة ميزة جديدة",
                ["Admin.ContentManagement.HomepageFeatures.EditDetails"] = "تعديل الميزة",
                ["Admin.ContentManagement.HomepageFeatures.BackToList"] = "العودة إلى مميزات الصفحة الرئيسية",
                ["Admin.ContentManagement.HomepageFeatures.Added"] = "تمت إضافة الميزة بنجاح.",
                ["Admin.ContentManagement.HomepageFeatures.Updated"] = "تم تحديث الميزة بنجاح.",
                ["Admin.ContentManagement.HomepageFeatures.Deleted"] = "تم حذف الميزة بنجاح.",
                ["Admin.ContentManagement.HomepageFeatures.Fields.Title"] = "العنوان",
                ["Admin.ContentManagement.HomepageFeatures.Fields.Title.Hint"] = "السطر العريض، مثل \"توصيل سريع\". يُفضّل كلمتان أو ثلاث.",
                ["Admin.ContentManagement.HomepageFeatures.Fields.Title.Required"] = "يرجى إدخال العنوان.",
                ["Admin.ContentManagement.HomepageFeatures.Fields.Hint"] = "السطر الفرعي",
                ["Admin.ContentManagement.HomepageFeatures.Fields.Hint.Hint"] = "السطر القصير تحت العنوان، مثل \"إلى باب منزلك\". اختياري.",
                ["Admin.ContentManagement.HomepageFeatures.Fields.Icon"] = "الأيقونة",
                ["Admin.ContentManagement.HomepageFeatures.Fields.Icon.Hint"] = "الصورة التي تظهر بجانب النص.",
                ["Admin.ContentManagement.HomepageFeatures.Fields.Published"] = "منشور",
                ["Admin.ContentManagement.HomepageFeatures.Fields.Published.Hint"] = "إظهار هذا العنصر في الصفحة الرئيسية.",
                ["Admin.ContentManagement.HomepageFeatures.Fields.DisplayOrder"] = "ترتيب العرض",
                ["Admin.ContentManagement.HomepageFeatures.Fields.DisplayOrder.Hint"] = "تُعرض العناصر من الرقم الأصغر إلى الأكبر.",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Delivery"] = "توصيل",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.CashOnDelivery"] = "نقود",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Genuine"] = "أصلي",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Support"] = "خدمة العملاء",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Returns"] = "إرجاع",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.SecurePayment"] = "دفع آمن",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Gift"] = "هدية",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Offers"] = "عروض",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Clock"] = "ساعة",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Star"] = "نجمة",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Phone"] = "هاتف",
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Store"] = "متجر"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
