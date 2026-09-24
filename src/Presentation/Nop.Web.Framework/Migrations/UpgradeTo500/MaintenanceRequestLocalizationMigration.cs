using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-12 00:00:00", "5.00", UpdateMigrationType.Localization)]
public class MaintenanceRequestLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //the maintenance request form, the live site's "طلب صيانة" page
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Literals.Nop.Core.Http.NopRouteNames.General.MaintenanceRequest"] = "Maintenance request",
            ["MaintenanceRequest.Area"] = "Street",
            ["MaintenanceRequest.Area.Hint"] = "Enter the street name",
            ["MaintenanceRequest.Area.Required"] = "Street is required",
            ["MaintenanceRequest.Brand"] = "Brand",
            ["MaintenanceRequest.Brand.Required"] = "Brand is required",
            ["MaintenanceRequest.Brand.Select"] = "Select a brand",
            ["MaintenanceRequest.Button"] = "Send",
            ["MaintenanceRequest.City"] = "City",
            ["MaintenanceRequest.City.Hint"] = "Enter the city name",
            ["MaintenanceRequest.City.Required"] = "City is required",
            ["MaintenanceRequest.ContactDetails"] = "Contact us",
            ["MaintenanceRequest.DeviceDetails"] = "Device details",
            ["MaintenanceRequest.DeviceType"] = "Device type",
            ["MaintenanceRequest.DeviceType.Hint"] = "e.g. split air conditioner",
            ["MaintenanceRequest.DeviceType.Required"] = "Device type is required",
            ["MaintenanceRequest.Email"] = "Email",
            ["MaintenanceRequest.Email.Hint"] = "example@gmail.com",
            ["MaintenanceRequest.EmailSubject"] = "New maintenance request",
            ["MaintenanceRequest.FullName"] = "Full name",
            ["MaintenanceRequest.FullName.Hint"] = "Enter your full name",
            ["MaintenanceRequest.FullName.Required"] = "Enter your name",
            ["MaintenanceRequest.InWarranty"] = "Within the warranty period",
            ["MaintenanceRequest.Location"] = "Address",
            ["MaintenanceRequest.MaintenancePolicy"] = "Maintenance policy",
            ["MaintenanceRequest.MaintenanceServices"] = "Out of warranty maintenance services",
            ["MaintenanceRequest.ModelNumber"] = "Model number",
            ["MaintenanceRequest.ModelNumber.Hint"] = "e.g. SWAC18WHC",
            ["MaintenanceRequest.ModelNumber.Required"] = "Model number is required",
            ["MaintenanceRequest.Note"] = "Note: dear customers, please be advised that there is no home service for small appliance repairs and the customer should visit the nearest authorised service centre. If the appliance is not under warranty, an inspection fee of 100 SAR is charged.",
            ["MaintenanceRequest.PhoneNumber"] = "Mobile",
            ["MaintenanceRequest.PhoneNumber.Hint"] = "05XXXXXXXX",
            ["MaintenanceRequest.PhoneNumber.Invalid"] = "The mobile number must be 10 digits and start with 05.",
            ["MaintenanceRequest.PhoneNumber.Required"] = "Enter your mobile number",
            ["MaintenanceRequest.Problem"] = "Write problem",
            ["MaintenanceRequest.Problem.Hint"] = "Please describe the problem ...",
            ["MaintenanceRequest.Problem.Required"] = "This field is required",
            ["MaintenanceRequest.YourRequestHasBeenSent"] = "The request has been successfully sent.",
            ["PageTitle.MaintenanceRequest"] = "Maintenance request",
        });

        //ponytail: match on the language prefix, not the exact culture - the pack ships ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is null)
            return;

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Literals.Nop.Core.Http.NopRouteNames.General.MaintenanceRequest"] = "طلب صيانة",
            ["MaintenanceRequest.Area"] = "الحيّ",
            ["MaintenanceRequest.Area.Hint"] = "ادخل اسم الحيّ",
            ["MaintenanceRequest.Area.Required"] = "المنطقة مطلوب",
            ["MaintenanceRequest.Brand"] = "العلامة التجارية",
            ["MaintenanceRequest.Brand.Required"] = "البراند مطلوب",
            ["MaintenanceRequest.Brand.Select"] = "اختر العلامة التجارية",
            ["MaintenanceRequest.Button"] = "إرســـال",
            ["MaintenanceRequest.City"] = "المدينة",
            ["MaintenanceRequest.City.Hint"] = "ادخل اسم المدينة",
            ["MaintenanceRequest.City.Required"] = "المدينة مطلوبة",
            ["MaintenanceRequest.ContactDetails"] = "تواصل معنا",
            ["MaintenanceRequest.DeviceDetails"] = "تفاصيل الجهاز",
            ["MaintenanceRequest.DeviceType"] = "نوع الجهاز",
            ["MaintenanceRequest.DeviceType.Hint"] = "مثال: مكيف سبليت",
            ["MaintenanceRequest.DeviceType.Required"] = "نوع الجهاز مطلوب",
            ["MaintenanceRequest.Email"] = "البريد الإلكتروني",
            ["MaintenanceRequest.Email.Hint"] = "example@gmail.com",
            ["MaintenanceRequest.EmailSubject"] = "طلب صيانة جديد",
            ["MaintenanceRequest.FullName"] = "الاسم الكامل",
            ["MaintenanceRequest.FullName.Hint"] = "اكتب اسمك الكامل",
            ["MaintenanceRequest.FullName.Required"] = "الاسم مطلوب",
            ["MaintenanceRequest.InWarranty"] = "ضمن فترة الضمان",
            ["MaintenanceRequest.Location"] = "العنوان",
            ["MaintenanceRequest.MaintenancePolicy"] = "سياسة الصيانة",
            ["MaintenanceRequest.MaintenanceServices"] = "خدمات الصيانة خارج الضمان",
            ["MaintenanceRequest.ModelNumber"] = "رقم الموديل",
            ["MaintenanceRequest.ModelNumber.Hint"] = "مثال: SWAC18WHC",
            ["MaintenanceRequest.ModelNumber.Required"] = "رقم الموديل مطلوب",
            ["MaintenanceRequest.Note"] = "ملاحظة: عملاءنا الكرام يرجاء العلم لا يوجد خدمة منزلية لصيانة الأجهزة الصغيرة وعلى العميل التوجه لأقرب صيانة معتمد, في حال عدم وجود ضمان, سيتم احتساب 100 ريال رسوم الفحص.",
            ["MaintenanceRequest.PhoneNumber"] = "رقم الجوال",
            ["MaintenanceRequest.PhoneNumber.Hint"] = "05XXXXXXXX",
            ["MaintenanceRequest.PhoneNumber.Invalid"] = "رقم الجوال يجب أن يتكون من 10 أرقام باللغة الإنجليزية فقط, وأن يبدأ ب 05.",
            ["MaintenanceRequest.PhoneNumber.Required"] = "رقم الهاتف مطلوب",
            ["MaintenanceRequest.Problem"] = "المشكلة",
            ["MaintenanceRequest.Problem.Hint"] = "الرجاء شرح المشكلة ...",
            ["MaintenanceRequest.Problem.Required"] = "هذا الحقل مطلوب",
            ["MaintenanceRequest.YourRequestHasBeenSent"] = "تم إرسال طلبك بنجاح",
            ["PageTitle.MaintenanceRequest"] = "طلب صيانة",
        }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}