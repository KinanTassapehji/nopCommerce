using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-08-31 00:00:02", "5.00", UpdateMigrationType.Localization)]
public class ShippingAddressLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //this store collects a single, shipping-only address - nothing is labelled "billing" any more
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.Orders.BillingShippingInfo"] = "Shipping",
            ["Admin.Orders.List.BillingEmail"] = "Email address",
            ["Admin.Orders.List.BillingEmail.Hint"] = "Filter by customer email address.",
            ["Admin.Orders.List.BillingLastName"] = "Last name",
            ["Admin.Orders.List.BillingLastName.Hint"] = "Filter by customer last name.",
            ["Admin.Orders.List.BillingPhone"] = "Phone number",
            ["Admin.Orders.List.BillingPhone.Hint"] = "Filter by customer phone number.",
            ["Admin.Orders.List.BillingCountry"] = "Shipping country",
            ["Admin.Orders.List.BillingCountry.Hint"] = "Filter by order shipping country.",
            ["Admin.Reports.Sales.Bestsellers.BillingCountry"] = "Shipping country",
            ["Admin.Reports.Sales.Bestsellers.BillingCountry.Hint"] = "Filter by order shipping country.",
            ["Admin.Reports.SalesSummary.BillingCountry"] = "Shipping country",
            ["Admin.Reports.SalesSummary.BillingCountry.Hint"] = "Filter by order shipping country.",
            ["Admin.ShoppingCartType.BillingCountry"] = "Shipping country",
            ["Admin.ShoppingCartType.BillingCountry.Hint"] = "Filter by shipping country.",
            ["Admin.Configuration.Settings.Order.DisableBillingAddressCheckoutStep"] = "Disable the address checkout step",
            ["Admin.Configuration.Settings.Order.DisableBillingAddressCheckoutStep.Hint"] = "Check to disable the \"Shipping address\" step during checkout. The address will be pre-filled and saved using the default registration data (this option cannot be used with guest checkout enabled). Also ensure that appropriate address fields that cannot be pre-filled are not required (or disabled). If a customer doesn't have an address, then the address step will be displayed."
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture == "ar-SA");
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.Orders.BillingShippingInfo"] = "الشحن",
                ["Admin.Orders.List.BillingEmail"] = "البريد الإلكتروني",
                ["Admin.Orders.List.BillingEmail.Hint"] = "تصفية حسب البريد الإلكتروني للعميل.",
                ["Admin.Orders.List.BillingLastName"] = "اسم العائلة",
                ["Admin.Orders.List.BillingLastName.Hint"] = "تصفية حسب اسم عائلة العميل.",
                ["Admin.Orders.List.BillingPhone"] = "رقم الهاتف",
                ["Admin.Orders.List.BillingPhone.Hint"] = "تصفية حسب رقم هاتف العميل.",
                ["Admin.Orders.List.BillingCountry"] = "بلد الشحن",
                ["Admin.Orders.List.BillingCountry.Hint"] = "تصفية حسب بلد شحن الطلب.",
                ["Admin.Reports.Sales.Bestsellers.BillingCountry"] = "بلد الشحن",
                ["Admin.Reports.Sales.Bestsellers.BillingCountry.Hint"] = "تصفية حسب بلد شحن الطلب.",
                ["Admin.Reports.SalesSummary.BillingCountry"] = "بلد الشحن",
                ["Admin.Reports.SalesSummary.BillingCountry.Hint"] = "تصفية حسب بلد شحن الطلب.",
                ["Admin.ShoppingCartType.BillingCountry"] = "بلد الشحن",
                ["Admin.ShoppingCartType.BillingCountry.Hint"] = "تصفية حسب بلد الشحن.",
                ["Admin.Configuration.Settings.Order.DisableBillingAddressCheckoutStep"] = "تعطيل خطوة العنوان أثناء الدفع",
                ["Admin.Configuration.Settings.Order.DisableBillingAddressCheckoutStep.Hint"] = "حدد لتعطيل خطوة \"عنوان الشحن\" أثناء الدفع. سيتم تعبئة العنوان مسبقاً وحفظه باستخدام بيانات التسجيل الافتراضية (لا يمكن استخدام هذا الخيار مع تفعيل الدفع كضيف). تأكد أيضاً من أن حقول العنوان التي لا يمكن تعبئتها مسبقاً غير مطلوبة (أو معطلة). إذا لم يكن لدى العميل عنوان، فسيتم عرض خطوة العنوان."
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}