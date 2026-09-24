using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Common;

/// <summary>
/// Represents a maintenance request model
/// </summary>
public partial record MaintenanceRequestModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.FullName")]
    public string FullName { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.PhoneNumber")]
    public string PhoneNumber { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.City")]
    public string City { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.Area")]
    public string Area { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.Brand")]
    public string Brand { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.DeviceType")]
    public string DeviceType { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.ModelNumber")]
    public string ModelNumber { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.InWarranty")]
    public bool InWarranty { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.Problem")]
    public string Problem { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.Status")]
    public int StatusId { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.Status")]
    public string StatusName { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.AdminComment")]
    public string AdminComment { get; set; }

    [NopResourceDisplayName("Admin.Sales.MaintenanceRequests.Fields.CreatedOn")]
    public DateTime CreatedOn { get; set; }

    public IList<SelectListItem> AvailableStatuses { get; set; } = new List<SelectListItem>();

    #endregion
}