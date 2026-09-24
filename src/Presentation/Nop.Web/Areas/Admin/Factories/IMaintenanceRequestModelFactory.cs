using Nop.Core.Domain.Common;
using Nop.Web.Areas.Admin.Models.Common;

namespace Nop.Web.Areas.Admin.Factories;

/// <summary>
/// Represents the maintenance request model factory
/// </summary>
public partial interface IMaintenanceRequestModelFactory
{
    /// <summary>
    /// Prepare maintenance request search model
    /// </summary>
    /// <param name="searchModel">Maintenance request search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the maintenance request search model
    /// </returns>
    Task<MaintenanceRequestSearchModel> PrepareMaintenanceRequestSearchModelAsync(MaintenanceRequestSearchModel searchModel);

    /// <summary>
    /// Prepare paged maintenance request list model
    /// </summary>
    /// <param name="searchModel">Maintenance request search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the maintenance request list model
    /// </returns>
    Task<MaintenanceRequestListModel> PrepareMaintenanceRequestListModelAsync(MaintenanceRequestSearchModel searchModel);

    /// <summary>
    /// Prepare maintenance request model
    /// </summary>
    /// <param name="model">Maintenance request model</param>
    /// <param name="maintenanceRequest">Maintenance request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the maintenance request model
    /// </returns>
    Task<MaintenanceRequestModel> PrepareMaintenanceRequestModelAsync(MaintenanceRequestModel model,
        MaintenanceRequest maintenanceRequest);
}