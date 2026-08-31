using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-08-24 00:00:01", "5.00", UpdateMigrationType.Localization)]
public class LocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //ponytail: brand name is the same in every language, so one call for all of them
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.PageTitle"] = "TmTm"
        });

        //gift cards and wishlists are not used in this store
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.CurrentCarts.CartsAndWishlists"] = "Shopping carts",
            ["Admin.Customers.Customers.ShoppingCartAndWishlist"] = "Current shopping cart"
        });

        //the home page ends with an endless catalog feed under its own heading
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Homepage.AllProducts"] = "Browse all products"
        });

        //WhatsApp joins the existing social links
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.Configuration.Settings.GeneralCommon.WhatsAppLink"] = "WhatsApp URL",
            ["Admin.Configuration.Settings.GeneralCommon.WhatsAppLink.Hint"] = "Specify your WhatsApp chat URL (e.g. https://wa.me/9665xxxxxxxx). Leave empty if you have no such account.",
            ["Footer.FollowUs.WhatsApp"] = "WhatsApp"
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.CurrentCarts.CartsAndWishlists"] = "عربات التسوق",
                ["Admin.Customers.Customers.ShoppingCartAndWishlist"] = "عربة التسوق الحالية",
                ["Admin.Configuration.Settings.GeneralCommon.WhatsAppLink"] = "رابط WhatsApp",
                ["Admin.Configuration.Settings.GeneralCommon.WhatsAppLink.Hint"] = "حدد رابط محادثة WhatsApp الخاص بك (مثال: https://wa.me/9665xxxxxxxx). اتركه فارغًا إذا لم يكن لديك حساب.",
                ["Footer.FollowUs.WhatsApp"] = "واتساب",
                ["Homepage.AllProducts"] = "تصفح كل المنتجات"
            }, arabic.Id);

        //product specifications are gone - the store describes a product with
        //product attributes only, so their resources have nothing left to label
        foreach (var prefix in new[]
                 {
                     "Admin.Catalog.Attributes.SpecificationAttributes",
                     "Admin.Catalog.Products.SpecificationAttributes",
                     "Admin.Configuration.Settings.Catalog.EnableSpecificationAttributeFiltering",
                     "Admin.Configuration.Settings.Catalog.ExportImportProductSpecificationAttributes",
                     "Admin.Configuration.Settings.ProductEditor.SpecificationAttributes",
                     "Admin.Documentation.Reference.SpecificationAttributes",
                     "Enums.Nop.Core.Domain.Catalog.SpecificationAttributeType",
                     "Filtering.SpecificationFilter",
                     "Products.Specs",
                     "Security.Permission.Catalog.SpecificationAttributes"
                 })
            localizationService.DeleteLocaleResourcesAsync(prefix).Wait();

        localizationService.DeleteLocaleResources(new[]
        {
            "ActivityLog.AddNewSpecAttribute",
            "ActivityLog.AddNewSpecAttributeGroup",
            "ActivityLog.DeleteSpecAttribute",
            "ActivityLog.DeleteSpecAttributeGroup",
            "ActivityLog.EditSpecAttribute",
            "ActivityLog.EditSpecAttributeGroup"
        });
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}