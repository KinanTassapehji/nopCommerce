using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Common;

public partial record MaintenanceRequestModel : BaseNopModel
{
    [NopResourceDisplayName("MaintenanceRequest.FullName")]
    public string FullName { get; set; }

    [DataType(DataType.PhoneNumber)]
    [NopResourceDisplayName("MaintenanceRequest.PhoneNumber")]
    public string PhoneNumber { get; set; }

    [DataType(DataType.EmailAddress)]
    [NopResourceDisplayName("MaintenanceRequest.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("MaintenanceRequest.City")]
    public string City { get; set; }

    [NopResourceDisplayName("MaintenanceRequest.Area")]
    public string Area { get; set; }

    [NopResourceDisplayName("MaintenanceRequest.Brand")]
    public string Brand { get; set; }

    [NopResourceDisplayName("MaintenanceRequest.DeviceType")]
    public string DeviceType { get; set; }

    [NopResourceDisplayName("MaintenanceRequest.InWarranty")]
    public bool InWarranty { get; set; }

    [NopResourceDisplayName("MaintenanceRequest.ModelNumber")]
    public string ModelNumber { get; set; }

    [NopResourceDisplayName("MaintenanceRequest.Problem")]
    public string Problem { get; set; }

    public bool SuccessfullySent { get; set; }
    public string Result { get; set; }

    public bool DisplayCaptcha { get; set; }
}