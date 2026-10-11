using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Three starter slides for the welcome tour, so the store has a first-visit screen the moment the
/// feature lands; the admin rewrites them under Content management > Welcome slides. Arabic is the
/// slide's own value (Arabic is the store's language), English its localized value.
/// </summary>
[NopUpdateMigration("2026-10-11 12:00:01", "5.00", UpdateMigrationType.Data)]
public class WelcomeSlideDataMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var repository = EngineContext.Current.Resolve<IRepository<WelcomeSlide>>();
        if (repository.Table.Any())
            return;

        var localizedEntityService = EngineContext.Current.Resolve<ILocalizedEntityService>();
        var english = EngineContext.Current.Resolve<ILanguageService>().GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("en", StringComparison.OrdinalIgnoreCase));

        var slides = new (string Title, string Text, string TitleEn, string TextEn, HomepageFeatureIcon Icon)[]
        {
            ("أهلاً بك في تمتم", "كل ما يحتاجه بيتك في مكان واحد. تسوّق بسهولة واستلم طلبك عند بابك.",
                "Welcome to tmtm", "Everything your home needs, in one place. Shop with ease and receive your order at your door.", HomepageFeatureIcon.Store),
            ("توصيل سريع إلى بابك", "اطلب الآن ونوصّل لك بأسرع وقت، وتابع طلبك خطوة بخطوة.",
                "Fast delivery to your door", "Order now, we deliver fast, and you follow your order step by step.", HomepageFeatureIcon.Delivery),
            ("ادفع عند الاستلام", "لا بطاقات ولا تعقيد. افحص طلبك عند وصوله ثم ادفع.",
                "Pay on delivery", "No cards, no hassle. Check your order when it arrives, then pay.", HomepageFeatureIcon.CashOnDelivery)
        };

        for (var i = 0; i < slides.Length; i++)
        {
            var (title, text, titleEn, textEn, icon) = slides[i];
            var slide = new WelcomeSlide
            {
                Title = title,
                Text = text,
                Icon = icon,
                Published = true,
                DisplayOrder = i + 1
            };
            repository.InsertAsync(slide, false).Wait();

            if (english is null)
                continue;

            localizedEntityService.SaveLocalizedValueAsync(slide, s => s.Title, titleEn, english.Id).Wait();
            localizedEntityService.SaveLocalizedValueAsync(slide, s => s.Text, textEn, english.Id).Wait();
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}