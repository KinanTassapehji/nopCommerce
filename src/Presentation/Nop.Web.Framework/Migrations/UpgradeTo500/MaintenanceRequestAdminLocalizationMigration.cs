using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-22 00:00:01", "5.00", UpdateMigrationType.Localization)]
public class MaintenanceRequestAdminLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //the admin list the maintenance requests land in
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.Sales.MaintenanceRequests"] = "Maintenance requests",
            ["Admin.Sales.MaintenanceRequests.BackToList"] = "back to maintenance request list",
            ["Admin.Sales.MaintenanceRequests.Customer"] = "Customer",
            ["Admin.Sales.MaintenanceRequests.Deleted"] = "The maintenance request has been deleted successfully.",
            ["Admin.Sales.MaintenanceRequests.Device"] = "Device",
            ["Admin.Sales.MaintenanceRequests.EditDetails"] = "Maintenance request",
            ["Admin.Sales.MaintenanceRequests.Fields.AdminComment"] = "Admin comment",
            ["Admin.Sales.MaintenanceRequests.Fields.AdminComment.Hint"] = "The note the staff keeps on this request. The customer never sees it.",
            ["Admin.Sales.MaintenanceRequests.Fields.Area"] = "Street",
            ["Admin.Sales.MaintenanceRequests.Fields.Brand"] = "Brand",
            ["Admin.Sales.MaintenanceRequests.Fields.City"] = "City",
            ["Admin.Sales.MaintenanceRequests.Fields.CreatedOn"] = "Created on",
            ["Admin.Sales.MaintenanceRequests.Fields.DeviceType"] = "Device type",
            ["Admin.Sales.MaintenanceRequests.Fields.Email"] = "Email",
            ["Admin.Sales.MaintenanceRequests.Fields.FullName"] = "Full name",
            ["Admin.Sales.MaintenanceRequests.Fields.InWarranty"] = "In warranty",
            ["Admin.Sales.MaintenanceRequests.Fields.ModelNumber"] = "Model number",
            ["Admin.Sales.MaintenanceRequests.Fields.PhoneNumber"] = "Mobile",
            ["Admin.Sales.MaintenanceRequests.Fields.Problem"] = "Problem",
            ["Admin.Sales.MaintenanceRequests.Fields.Status"] = "Status",
            ["Admin.Sales.MaintenanceRequests.Fields.Status.Hint"] = "Where this request stands.",
            ["Admin.Sales.MaintenanceRequests.Handling"] = "Handling",
            ["Admin.Sales.MaintenanceRequests.List.AdminComment"] = "Admin comment",
            ["Admin.Sales.MaintenanceRequests.List.AdminComment.Hint"] = "Search in the staff notes.",
            ["Admin.Sales.MaintenanceRequests.List.Area"] = "Street",
            ["Admin.Sales.MaintenanceRequests.List.Area.Hint"] = "Search by street.",
            ["Admin.Sales.MaintenanceRequests.List.Brand"] = "Brand",
            ["Admin.Sales.MaintenanceRequests.List.Brand.Hint"] = "Show only the requests for this brand.",
            ["Admin.Sales.MaintenanceRequests.List.City"] = "City",
            ["Admin.Sales.MaintenanceRequests.List.City.Hint"] = "Search by city.",
            ["Admin.Sales.MaintenanceRequests.List.DeviceType"] = "Device type",
            ["Admin.Sales.MaintenanceRequests.List.DeviceType.Hint"] = "Search by device type.",
            ["Admin.Sales.MaintenanceRequests.List.Email"] = "Email",
            ["Admin.Sales.MaintenanceRequests.List.Email.Hint"] = "Search by email.",
            ["Admin.Sales.MaintenanceRequests.List.EndDate"] = "End date",
            ["Admin.Sales.MaintenanceRequests.List.EndDate.Hint"] = "The last day of the period.",
            ["Admin.Sales.MaintenanceRequests.List.FullName"] = "Full name",
            ["Admin.Sales.MaintenanceRequests.List.FullName.Hint"] = "Search by customer name.",
            ["Admin.Sales.MaintenanceRequests.List.InWarranty"] = "Warranty",
            ["Admin.Sales.MaintenanceRequests.List.InWarranty.Hint"] = "Show only the devices in, or out of, the warranty period.",
            ["Admin.Sales.MaintenanceRequests.List.Keywords"] = "Keywords",
            ["Admin.Sales.MaintenanceRequests.List.Keywords.Hint"] = "Search every field of the request at once.",
            ["Admin.Sales.MaintenanceRequests.List.ModelNumber"] = "Model number",
            ["Admin.Sales.MaintenanceRequests.List.ModelNumber.Hint"] = "Search by model number.",
            ["Admin.Sales.MaintenanceRequests.List.PhoneNumber"] = "Mobile",
            ["Admin.Sales.MaintenanceRequests.List.PhoneNumber.Hint"] = "Search by mobile number.",
            ["Admin.Sales.MaintenanceRequests.List.Problem"] = "Problem",
            ["Admin.Sales.MaintenanceRequests.List.Problem.Hint"] = "Search in the problem description.",
            ["Admin.Sales.MaintenanceRequests.List.StartDate"] = "Start date",
            ["Admin.Sales.MaintenanceRequests.List.StartDate.Hint"] = "The first day of the period.",
            ["Admin.Sales.MaintenanceRequests.List.Status"] = "Status",
            ["Admin.Sales.MaintenanceRequests.List.Status.Hint"] = "Show only the requests with this status.",
            ["Admin.Sales.MaintenanceRequests.Problem"] = "Problem as the customer described it",
            ["Admin.Sales.MaintenanceRequests.Updated"] = "The maintenance request has been updated successfully.",
            ["Enums.Nop.Core.Domain.Common.MaintenanceRequestStatus.Cancelled"] = "Cancelled",
            ["Enums.Nop.Core.Domain.Common.MaintenanceRequestStatus.Completed"] = "Completed",
            ["Enums.Nop.Core.Domain.Common.MaintenanceRequestStatus.InProgress"] = "In progress",
            ["Enums.Nop.Core.Domain.Common.MaintenanceRequestStatus.New"] = "New",
        });

        //ponytail: match on the language prefix, not the exact culture - the pack ships ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is null)
            return;

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.Sales.MaintenanceRequests"] = "طلبات الصيانة",
            ["Admin.Sales.MaintenanceRequests.BackToList"] = "العودة إلى قائمة طلبات الصيانة",
            ["Admin.Sales.MaintenanceRequests.Customer"] = "العميل",
            ["Admin.Sales.MaintenanceRequests.Deleted"] = "تم حذف طلب الصيانة بنجاح.",
            ["Admin.Sales.MaintenanceRequests.Device"] = "الجهاز",
            ["Admin.Sales.MaintenanceRequests.EditDetails"] = "طلب صيانة",
            ["Admin.Sales.MaintenanceRequests.Fields.AdminComment"] = "ملاحظة الإدارة",
            ["Admin.Sales.MaintenanceRequests.Fields.AdminComment.Hint"] = "ملاحظة داخلية للموظفين، لا يراها العميل.",
            ["Admin.Sales.MaintenanceRequests.Fields.Area"] = "الحيّ",
            ["Admin.Sales.MaintenanceRequests.Fields.Brand"] = "العلامة التجارية",
            ["Admin.Sales.MaintenanceRequests.Fields.City"] = "المدينة",
            ["Admin.Sales.MaintenanceRequests.Fields.CreatedOn"] = "تاريخ الطلب",
            ["Admin.Sales.MaintenanceRequests.Fields.DeviceType"] = "نوع الجهاز",
            ["Admin.Sales.MaintenanceRequests.Fields.Email"] = "البريد الالكتروني",
            ["Admin.Sales.MaintenanceRequests.Fields.FullName"] = "الاسم الكامل",
            ["Admin.Sales.MaintenanceRequests.Fields.InWarranty"] = "ضمن فترة الضمان",
            ["Admin.Sales.MaintenanceRequests.Fields.ModelNumber"] = "رقم الموديل",
            ["Admin.Sales.MaintenanceRequests.Fields.PhoneNumber"] = "رقم الجوال",
            ["Admin.Sales.MaintenanceRequests.Fields.Problem"] = "المشكلة",
            ["Admin.Sales.MaintenanceRequests.Fields.Status"] = "الحالة",
            ["Admin.Sales.MaintenanceRequests.Fields.Status.Hint"] = "حالة الطلب.",
            ["Admin.Sales.MaintenanceRequests.Handling"] = "المتابعة",
            ["Admin.Sales.MaintenanceRequests.List.AdminComment"] = "ملاحظة الإدارة",
            ["Admin.Sales.MaintenanceRequests.List.AdminComment.Hint"] = "البحث في ملاحظات الموظفين.",
            ["Admin.Sales.MaintenanceRequests.List.Area"] = "الحيّ",
            ["Admin.Sales.MaintenanceRequests.List.Area.Hint"] = "البحث بالحيّ.",
            ["Admin.Sales.MaintenanceRequests.List.Brand"] = "العلامة التجارية",
            ["Admin.Sales.MaintenanceRequests.List.Brand.Hint"] = "عرض طلبات هذه العلامة التجارية فقط.",
            ["Admin.Sales.MaintenanceRequests.List.City"] = "المدينة",
            ["Admin.Sales.MaintenanceRequests.List.City.Hint"] = "البحث بالمدينة.",
            ["Admin.Sales.MaintenanceRequests.List.DeviceType"] = "نوع الجهاز",
            ["Admin.Sales.MaintenanceRequests.List.DeviceType.Hint"] = "البحث بنوع الجهاز.",
            ["Admin.Sales.MaintenanceRequests.List.Email"] = "البريد الالكتروني",
            ["Admin.Sales.MaintenanceRequests.List.Email.Hint"] = "البحث بالبريد الالكتروني.",
            ["Admin.Sales.MaintenanceRequests.List.EndDate"] = "تاريخ النهاية",
            ["Admin.Sales.MaintenanceRequests.List.EndDate.Hint"] = "آخر يوم في الفترة.",
            ["Admin.Sales.MaintenanceRequests.List.FullName"] = "الاسم الكامل",
            ["Admin.Sales.MaintenanceRequests.List.FullName.Hint"] = "البحث باسم العميل.",
            ["Admin.Sales.MaintenanceRequests.List.InWarranty"] = "الضمان",
            ["Admin.Sales.MaintenanceRequests.List.InWarranty.Hint"] = "عرض الأجهزة ضمن فترة الضمان أو خارجها فقط.",
            ["Admin.Sales.MaintenanceRequests.List.Keywords"] = "كلمات البحث",
            ["Admin.Sales.MaintenanceRequests.List.Keywords.Hint"] = "البحث في كل حقول الطلب دفعة واحدة.",
            ["Admin.Sales.MaintenanceRequests.List.ModelNumber"] = "رقم الموديل",
            ["Admin.Sales.MaintenanceRequests.List.ModelNumber.Hint"] = "البحث برقم الموديل.",
            ["Admin.Sales.MaintenanceRequests.List.PhoneNumber"] = "رقم الجوال",
            ["Admin.Sales.MaintenanceRequests.List.PhoneNumber.Hint"] = "البحث برقم الجوال.",
            ["Admin.Sales.MaintenanceRequests.List.Problem"] = "المشكلة",
            ["Admin.Sales.MaintenanceRequests.List.Problem.Hint"] = "البحث في وصف المشكلة.",
            ["Admin.Sales.MaintenanceRequests.List.StartDate"] = "تاريخ البداية",
            ["Admin.Sales.MaintenanceRequests.List.StartDate.Hint"] = "أول يوم في الفترة.",
            ["Admin.Sales.MaintenanceRequests.List.Status"] = "الحالة",
            ["Admin.Sales.MaintenanceRequests.List.Status.Hint"] = "عرض الطلبات بهذه الحالة فقط.",
            ["Admin.Sales.MaintenanceRequests.Problem"] = "المشكلة كما وصفها العميل",
            ["Admin.Sales.MaintenanceRequests.Updated"] = "تم تحديث طلب الصيانة بنجاح.",
            ["Enums.Nop.Core.Domain.Common.MaintenanceRequestStatus.Cancelled"] = "ملغي",
            ["Enums.Nop.Core.Domain.Common.MaintenanceRequestStatus.Completed"] = "منجز",
            ["Enums.Nop.Core.Domain.Common.MaintenanceRequestStatus.InProgress"] = "قيد التنفيذ",
            ["Enums.Nop.Core.Domain.Common.MaintenanceRequestStatus.New"] = "جديد",
        }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
