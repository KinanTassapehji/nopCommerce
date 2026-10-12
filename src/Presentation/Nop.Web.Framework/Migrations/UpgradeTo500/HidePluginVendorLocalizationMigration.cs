using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The customer role page shows plugin permissions by what they do, not by the plugin vendor's name
/// </summary>
[NopUpdateMigration("2026-10-05 18:23:00", "5.00", UpdateMigrationType.Localization)]
public class HidePluginVendorLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.Customers.CustomerRoles.Permissions.Category.NopStation"] = "Extra features",
            ["Security.Permission.ManageAdminReportExporter"] = "Report export. Manage",
            ["Security.Permission.ManageAdminReportExporterConfiguration"] = "Report export. Settings",
            ["Security.Permission.ManageCustomerReminders"] = "Customer reminders. Manage",
            ["Security.Permission.ManageNopStationCancelOrder"] = "Order cancellation. Manage",
            ["Security.Permission.ManageNopStationCoreConfiguration"] = "Add-ons. Settings",
            ["Security.Permission.ManageNopStationCoreLicense"] = "Add-ons. Licenses",
            ["Security.Permission.ManageNopStationFeatures"] = "Add-ons. Features",
            ["Security.Permission.ShowNopStationDocumentations"] = "Add-ons. Help and documentation",
            ["Security.Permission.ManageNopStationSmsConfiguration"] = "SMS. Settings",
            ["Security.Permission.ManageNopStationSmsProviders"] = "SMS. Providers",
            ["Security.Permission.ManageNopStationSmsQueue"] = "SMS. Queue",
            ["Security.Permission.ManageNopStationSmsTemplates"] = "SMS. Templates",
            ["Security.Permission.ManageNopStationOCarousels"] = "Product carousels. Manage",
            ["Security.Permission.ManageNopStationPictureZoom"] = "Picture zoom. Manage",
            ["Security.Permission.ManageNopStationProduct360View"] = "360° product view. Manage",
            ["Security.Permission.ManageNopStationProductRibbon"] = "Product ribbons. Manage",
            ["Security.Permission.ManageNopStationProductTab"] = "Product tabs. Manage",
            ["Security.Permission.ManageNopStationSliders"] = "Sliders. Manage",
            ["Security.Permission.ManageNopStationMegaMenu"] = "Mega menu. Manage",
            ["Security.Permission.ManageNopStationPrevNextProduct"] = "Previous/next product. Manage",
            ["Security.Permission.ManageNopStationQuickView"] = "Quick view. Manage",
            ["Admin.Customers.CustomerRoles.Permissions.Description.ManageNopStationCoreConfiguration"] = "Change the settings shared by the store's add-ons.",
            ["Admin.Customers.CustomerRoles.Permissions.Description.ManageNopStationCoreLicense"] = "See and change the add-on licenses.",
            ["Admin.Customers.CustomerRoles.Permissions.Description.ManageNopStationFeatures"] = "Turn add-on features on or off.",
            ["Admin.Customers.CustomerRoles.Permissions.Description.ShowNopStationDocumentations"] = "See the add-ons' help and documentation links."
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.Customers.CustomerRoles.Permissions.Category.NopStation"] = "ميزات إضافية",
                ["Security.Permission.ManageAdminReportExporter"] = "تصدير التقارير. إدارة",
                ["Security.Permission.ManageAdminReportExporterConfiguration"] = "تصدير التقارير. الإعدادات",
                ["Security.Permission.ManageCustomerReminders"] = "تذكيرات العملاء. إدارة",
                ["Security.Permission.ManageNopStationCancelOrder"] = "إلغاء الطلبات. إدارة",
                ["Security.Permission.ManageNopStationCoreConfiguration"] = "الإضافات. الإعدادات",
                ["Security.Permission.ManageNopStationCoreLicense"] = "الإضافات. التراخيص",
                ["Security.Permission.ManageNopStationFeatures"] = "الإضافات. الميزات",
                ["Security.Permission.ShowNopStationDocumentations"] = "الإضافات. المساعدة والتوثيق",
                ["Security.Permission.ManageNopStationSmsConfiguration"] = "الرسائل النصية. الإعدادات",
                ["Security.Permission.ManageNopStationSmsProviders"] = "الرسائل النصية. المزوّدون",
                ["Security.Permission.ManageNopStationSmsQueue"] = "الرسائل النصية. قائمة الانتظار",
                ["Security.Permission.ManageNopStationSmsTemplates"] = "الرسائل النصية. القوالب",
                ["Security.Permission.ManageNopStationOCarousels"] = "عارضات المنتجات الدوّارة. إدارة",
                ["Security.Permission.ManageNopStationPictureZoom"] = "تكبير الصور. إدارة",
                ["Security.Permission.ManageNopStationProduct360View"] = "عرض المنتج 360°. إدارة",
                ["Security.Permission.ManageNopStationProductRibbon"] = "أشرطة المنتجات. إدارة",
                ["Security.Permission.ManageNopStationProductTab"] = "تبويبات المنتج. إدارة",
                ["Security.Permission.ManageNopStationSliders"] = "العارضات المتحركة. إدارة",
                ["Security.Permission.ManageNopStationMegaMenu"] = "القائمة الكبرى. إدارة",
                ["Security.Permission.ManageNopStationPrevNextProduct"] = "المنتج السابق/التالي. إدارة",
                ["Security.Permission.ManageNopStationQuickView"] = "العرض السريع. إدارة",
                ["Admin.Customers.CustomerRoles.Permissions.Description.ManageNopStationCoreConfiguration"] = "تغيير الإعدادات المشتركة لإضافات المتجر.",
                ["Admin.Customers.CustomerRoles.Permissions.Description.ManageNopStationCoreLicense"] = "عرض تراخيص الإضافات وتغييرها.",
                ["Admin.Customers.CustomerRoles.Permissions.Description.ManageNopStationFeatures"] = "تفعيل ميزات الإضافات أو إيقافها.",
                ["Admin.Customers.CustomerRoles.Permissions.Description.ShowNopStationDocumentations"] = "عرض روابط المساعدة والتوثيق الخاصة بالإضافات."
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
