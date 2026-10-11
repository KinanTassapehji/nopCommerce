using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Labels of Content management > Welcome slides, and the tour's own buttons
/// </summary>
[NopUpdateMigration("2026-10-11 12:00:02", "5.00", UpdateMigrationType.Localization)]
public class WelcomeSlideLocalizationMigration : MigrationBase
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
            ["Admin.ContentManagement.WelcomeSlides"] = "Welcome slides",
            ["Admin.ContentManagement.WelcomeSlides.Hint"] = "The short tour a visitor sees once, on the first visit to the home page: a picture, a title and a line per slide, with Skip and Next. Unpublish a slide to hide it without deleting it; with no published slides the tour is off.",
            ["Admin.ContentManagement.WelcomeSlides.AddNew"] = "Add a new welcome slide",
            ["Admin.ContentManagement.WelcomeSlides.EditDetails"] = "Edit welcome slide",
            ["Admin.ContentManagement.WelcomeSlides.BackToList"] = "back to welcome slides",
            ["Admin.ContentManagement.WelcomeSlides.Added"] = "The welcome slide has been added successfully.",
            ["Admin.ContentManagement.WelcomeSlides.Updated"] = "The welcome slide has been updated successfully.",
            ["Admin.ContentManagement.WelcomeSlides.Deleted"] = "The welcome slide has been deleted successfully.",
            ["Admin.ContentManagement.WelcomeSlides.Preview"] = "Preview the tour",
            ["Admin.ContentManagement.WelcomeSlides.Fields.Title"] = "Title",
            ["Admin.ContentManagement.WelcomeSlides.Fields.Title.Hint"] = "The big line, e.g. \"Welcome to tmtm\". Keep it short.",
            ["Admin.ContentManagement.WelcomeSlides.Fields.Title.Required"] = "Please enter a title.",
            ["Admin.ContentManagement.WelcomeSlides.Fields.Text"] = "Text",
            ["Admin.ContentManagement.WelcomeSlides.Fields.Text.Hint"] = "One or two sentences under the title. Optional.",
            ["Admin.ContentManagement.WelcomeSlides.Fields.Picture"] = "Picture",
            ["Admin.ContentManagement.WelcomeSlides.Fields.Picture.Hint"] = "The picture on the slide. Square or portrait works best; a PNG with a transparent background sits on the brand colour. Without a picture the icon below is drawn instead.",
            ["Admin.ContentManagement.WelcomeSlides.Fields.Icon"] = "Icon",
            ["Admin.ContentManagement.WelcomeSlides.Fields.Icon.Hint"] = "Drawn when the slide has no picture.",
            ["Admin.ContentManagement.WelcomeSlides.Fields.Published"] = "Published",
            ["Admin.ContentManagement.WelcomeSlides.Fields.Published.Hint"] = "Show this slide in the tour.",
            ["Admin.ContentManagement.WelcomeSlides.Fields.DisplayOrder"] = "Display order",
            ["Admin.ContentManagement.WelcomeSlides.Fields.DisplayOrder.Hint"] = "Slides are shown from the lowest number to the highest.",
            ["Welcome.Skip"] = "Skip",
            ["Welcome.Next"] = "Next",
            ["Welcome.Start"] = "Start shopping"
        });

        //ponytail: match on the language prefix - the packs ship as ar-SY and ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.ContentManagement.WelcomeSlides"] = "شرائح الترحيب",
                ["Admin.ContentManagement.WelcomeSlides.Hint"] = "الجولة القصيرة التي يراها الزائر مرة واحدة عند أول زيارة للصفحة الرئيسية: صورة وعنوان وسطر لكل شريحة، مع زرّي تخطي والتالي. ألغِ نشر شريحة لإخفائها دون حذفها؛ وبلا شرائح منشورة تتوقف الجولة.",
                ["Admin.ContentManagement.WelcomeSlides.AddNew"] = "إضافة شريحة ترحيب جديدة",
                ["Admin.ContentManagement.WelcomeSlides.EditDetails"] = "تعديل شريحة الترحيب",
                ["Admin.ContentManagement.WelcomeSlides.BackToList"] = "العودة إلى شرائح الترحيب",
                ["Admin.ContentManagement.WelcomeSlides.Added"] = "تمت إضافة الشريحة بنجاح.",
                ["Admin.ContentManagement.WelcomeSlides.Updated"] = "تم تحديث الشريحة بنجاح.",
                ["Admin.ContentManagement.WelcomeSlides.Deleted"] = "تم حذف الشريحة بنجاح.",
                ["Admin.ContentManagement.WelcomeSlides.Preview"] = "معاينة الجولة",
                ["Admin.ContentManagement.WelcomeSlides.Fields.Title"] = "العنوان",
                ["Admin.ContentManagement.WelcomeSlides.Fields.Title.Hint"] = "السطر الكبير، مثل \"أهلاً بك في تمتم\". يُفضّل أن يكون قصيراً.",
                ["Admin.ContentManagement.WelcomeSlides.Fields.Title.Required"] = "يرجى إدخال العنوان.",
                ["Admin.ContentManagement.WelcomeSlides.Fields.Text"] = "النص",
                ["Admin.ContentManagement.WelcomeSlides.Fields.Text.Hint"] = "جملة أو جملتان تحت العنوان. اختياري.",
                ["Admin.ContentManagement.WelcomeSlides.Fields.Picture"] = "الصورة",
                ["Admin.ContentManagement.WelcomeSlides.Fields.Picture.Hint"] = "صورة الشريحة. الأفضل مربعة أو طولية؛ صورة PNG بخلفية شفافة تظهر على لون العلامة. بدون صورة تُرسم الأيقونة أدناه.",
                ["Admin.ContentManagement.WelcomeSlides.Fields.Icon"] = "الأيقونة",
                ["Admin.ContentManagement.WelcomeSlides.Fields.Icon.Hint"] = "تُرسم عندما لا تحتوي الشريحة على صورة.",
                ["Admin.ContentManagement.WelcomeSlides.Fields.Published"] = "منشورة",
                ["Admin.ContentManagement.WelcomeSlides.Fields.Published.Hint"] = "إظهار هذه الشريحة في الجولة.",
                ["Admin.ContentManagement.WelcomeSlides.Fields.DisplayOrder"] = "ترتيب العرض",
                ["Admin.ContentManagement.WelcomeSlides.Fields.DisplayOrder.Hint"] = "تُعرض الشرائح من الرقم الأصغر إلى الأكبر.",
                ["Welcome.Skip"] = "تخطي",
                ["Welcome.Next"] = "التالي",
                ["Welcome.Start"] = "ابدأ التسوق"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}