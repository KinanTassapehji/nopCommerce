using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Common;

/// <summary>
/// Represents a maintenance request search model
/// </summary>
public partial record MaintenanceRequestSearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.StartDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchStartDate { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.EndDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchEndDate { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.Status")]
    public int SearchStatusId { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.InWarranty")]
    public int SearchInWarrantyId { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.Keywords")]
    public string SearchKeywords { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.FullName")]
    public string SearchFullName { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.PhoneNumber")]
    public string SearchPhoneNumber { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.Email")]
    public string SearchEmail { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.City")]
    public string SearchCity { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.Area")]
    public string SearchArea { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.Brand")]
    public string SearchBrand { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.DeviceType")]
    public string SearchDeviceType { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.ModelNumber")]
    public string SearchModelNumber { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.Problem")]
    public string SearchProblem { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.List.AdminComment")]
    public string SearchAdminComment { get; set; }

    public IList<SelectListItem> AvailableStatuses { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableWarrantyOptions { get; set; } = new List<SelectListItem>();

    //ponytail: the brand list comes from what has been sent in, not from the public form's own
    //options - those can change, and old requests keep the value they were sent with. City stays
    //a text box: it is typed, so it is spelled every which way and a list of it would be noise
    public IList<SelectListItem> AvailableBrands { get; set; } = new List<SelectListItem>();

    #endregion
}
