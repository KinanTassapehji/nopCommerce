using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Common;

/// <summary>
/// Represents a maintenance request list model
/// </summary>
public partial record MaintenanceRequestListModel : BasePagedListModel<MaintenanceRequestModel>
{
}