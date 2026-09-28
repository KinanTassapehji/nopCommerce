using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-26 23:17:00", "5.00", UpdateMigrationType.Localization)]
public class AdminMenuReorganizationLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //the new Marketing group, and plain names for what the store staff see in the sidebar
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.Marketing"] = "Marketing",
            ["Admin.ContentManagement.Topics"] = "Pages",
            ["Admin.NopStation.Core.LocaleResources"] = "Site texts",
            ["Admin.NopStation.Core.Menu.LocaleResources"] = "Site texts",
            ["Admin.NopStation.ProductTabs.Menu.ProductTab"] = "Product tabs",
            ["Plugins.Widgets.FirebasePushNotification.Broadcast.PageTitle"] = "Push notifications"
        });

        //besides the renames: "measures" read as "procedures", "pickup points" as "dispatch points",
        //"catalog" had the same name as its own "products" page, and customers were called two different words
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.Marketing"] = "التسويق",
                ["Admin.ContentManagement.Topics"] = "الصفحات",
                ["Admin.Catalog"] = "الكتالوج",
                ["Admin.Catalog.Manufacturers"] = "العلامات التجارية",
                ["Admin.Catalog.Products.Fields.Manufacturers"] = "العلامات التجارية",
                ["Admin.Configuration.Settings.ProductEditor.Manufacturers"] = "العلامات التجارية",
                ["Admin.Catalog.ProductTags"] = "الوسوم",
                ["Admin.Catalog.Products.Fields.ProductTags"] = "الوسوم",
                ["Admin.Configuration.Settings.ProductEditor.ProductTags"] = "الوسوم",
                ["Admin.Orders.Shipments"] = "الشحنات",
                ["Admin.Orders.Shipments.List"] = "الشحنات",
                ["Admin.Reports.Sales.NeverSold"] = "منتجات لم تُبع",
                ["Admin.Reports.Customers.RegisteredCustomers"] = "العملاء المسجلون",
                ["Admin.Dashboard.NumberOfCustomers"] = "العملاء المسجلون",
                ["Admin.Reports.Customers.BestBy.BestByOrderTotal"] = "العملاء حسب إجمالي الطلبات",
                ["Admin.Reports.Customers.BestBy.BestByNumberOfOrders"] = "العملاء حسب عدد الطلبات",
                ["Admin.ContentManagement.MessageTemplates"] = "قوالب الرسائل",
                ["Admin.Configuration"] = "الإعدادات",
                ["Admin.Configuration.Languages"] = "اللغات",
                ["Admin.Configuration.Shipping.PickupPoints"] = "نقاط الاستلام",
                ["Admin.Configuration.Shipping.DatesAndRanges"] = "مواعيد التوصيل",
                ["Admin.Configuration.Shipping.Measures"] = "وحدات القياس",
                ["Admin.NopStation.Core.LocaleResources"] = "نصوص الموقع",
                ["Admin.NopStation.Core.Menu.LocaleResources"] = "نصوص الموقع",
                ["Admin.NopStation.AnywhereSlider.Menu.Sliders"] = "السلايدر",
                ["Admin.NopStation.AnywhereSlider.SliderList"] = "السلايدر",
                ["Admin.NopStation.OCarousels.Menu.Carousels"] = "شرائط المنتجات",
                ["Admin.NopStation.OCarousels.CarouselList"] = "شرائط المنتجات",
                ["Admin.NopStation.ProductTabs.Menu.ProductTab"] = "تبويبات المنتجات",
                ["Admin.NopStation.ProductTabs.ProductTabList"] = "تبويبات المنتجات",
                ["Plugins.Widgets.FirebasePushNotification.Broadcast.PageTitle"] = "الإشعارات الفورية"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}