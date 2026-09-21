using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-20 00:00:04", "5.00", UpdateMigrationType.Localization)]
public class HomepageMarketingLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //the home page marketing bands: trust strip, category heading, deals, call to action
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Homepage.Categories"] = "Shop by category",
            ["Homepage.ViewAll"] = "View all",
            ["Homepage.Deals"] = "Deals",
            ["Homepage.Usp.Delivery"] = "Fast delivery",
            ["Homepage.Usp.Delivery.Hint"] = "Straight to your door",
            ["Homepage.Usp.Cod"] = "Cash on delivery",
            ["Homepage.Usp.Cod.Hint"] = "Pay when your order arrives",
            ["Homepage.Usp.Genuine"] = "Genuine products",
            ["Homepage.Usp.Genuine.Hint"] = "Quality you can count on",
            ["Homepage.Usp.Support"] = "Customer care",
            ["Homepage.Usp.Support.Hint"] = "We are here to help",
            ["Homepage.Cta.Title"] = "Everything your home needs, in one place",
            ["Homepage.Cta.Text"] = "Browse the full range and find your next favourite.",
            ["Homepage.Cta.Button"] = "Shop now",
            ["Homepage.AllProducts.Loading"] = "Loading more products"
        });

        //ponytail: match on the language prefix - the packs ship as ar-SY and ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Homepage.Categories"] = "تسوق حسب الفئة",
                ["Homepage.ViewAll"] = "عرض الكل",
                ["Homepage.Deals"] = "عروض وخصومات",
                ["Homepage.Usp.Delivery"] = "توصيل سريع",
                ["Homepage.Usp.Delivery.Hint"] = "إلى باب منزلك",
                ["Homepage.Usp.Cod"] = "الدفع عند الاستلام",
                ["Homepage.Usp.Cod.Hint"] = "ادفع عند وصول طلبك",
                ["Homepage.Usp.Genuine"] = "منتجات أصلية",
                ["Homepage.Usp.Genuine.Hint"] = "جودة تعتمد عليها",
                ["Homepage.Usp.Support"] = "خدمة العملاء",
                ["Homepage.Usp.Support.Hint"] = "نحن هنا لمساعدتك",
                ["Homepage.Cta.Title"] = "كل ما يحتاجه منزلك في مكان واحد",
                ["Homepage.Cta.Text"] = "تصفح المجموعة الكاملة واكتشف ما يناسبك.",
                ["Homepage.Cta.Button"] = "تسوق الآن",
                ["Homepage.AllProducts.Loading"] = "جارٍ تحميل المزيد من المنتجات"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
