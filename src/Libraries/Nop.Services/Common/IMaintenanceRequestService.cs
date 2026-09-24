using Nop.Core;
using Nop.Core.Domain.Common;

namespace Nop.Services.Common;

/// <summary>
/// Maintenance request service interface
/// </summary>
public partial interface IMaintenanceRequestService
{
    /// <summary>
    /// Inserts a maintenance request
    /// </summary>
    /// <param name="maintenanceRequest">Maintenance request</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertMaintenanceRequestAsync(MaintenanceRequest maintenanceRequest);

    /// <summary>
    /// Updates a maintenance request
    /// </summary>
    /// <param name="maintenanceRequest">Maintenance request</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateMaintenanceRequestAsync(MaintenanceRequest maintenanceRequest);

    /// <summary>
    /// Deletes a maintenance request
    /// </summary>
    /// <param name="maintenanceRequest">Maintenance request</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteMaintenanceRequestAsync(MaintenanceRequest maintenanceRequest);

    /// <summary>
    /// Deletes maintenance requests
    /// </summary>
    /// <param name="maintenanceRequests">Maintenance requests</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteMaintenanceRequestsAsync(IList<MaintenanceRequest> maintenanceRequests);

    /// <summary>
    /// Gets a maintenance request
    /// </summary>
    /// <param name="maintenanceRequestId">Maintenance request identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the maintenance request
    /// </returns>
    Task<MaintenanceRequest> GetMaintenanceRequestByIdAsync(int maintenanceRequestId);

    /// <summary>
    /// Gets maintenance requests by identifiers
    /// </summary>
    /// <param name="maintenanceRequestIds">Maintenance request identifiers</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the maintenance requests
    /// </returns>
    Task<IList<MaintenanceRequest>> GetMaintenanceRequestsByIdsAsync(int[] maintenanceRequestIds);

    /// <summary>
    /// Gets the values a column has actually been filled with, so a filter can offer real choices
    /// </summary>
    /// <param name="selector">Column to collect</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the distinct values, sorted
    /// </returns>
    Task<IList<string>> GetUsedValuesAsync(Func<IQueryable<MaintenanceRequest>, IQueryable<string>> selector);

    /// <summary>
    /// Searches maintenance requests, newest first
    /// </summary>
    /// <param name="createdFromUtc">Created date from (UTC); pass null to load all records</param>
    /// <param name="createdToUtc">Created date to (UTC); pass null to load all records</param>
    /// <param name="statusId">Status identifier; pass null to load all records</param>
    /// <param name="inWarranty">Warranty state; pass null to load all records</param>
    /// <param name="fullName">Full name; pass null to load all records</param>
    /// <param name="phoneNumber">Mobile number; pass null to load all records</param>
    /// <param name="email">Email; pass null to load all records</param>
    /// <param name="city">City; pass null to load all records</param>
    /// <param name="area">Street; pass null to load all records</param>
    /// <param name="brand">Brand, matched in full; pass null to load all records</param>
    /// <param name="deviceType">Device type; pass null to load all records</param>
    /// <param name="modelNumber">Model number; pass null to load all records</param>
    /// <param name="problem">Text in the problem description; pass null to load all records</param>
    /// <param name="adminComment">Text in the admin comment; pass null to load all records</param>
    /// <param name="keywords">Text in any of the fields above; pass null to load all records</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the paged list of maintenance requests
    /// </returns>
    Task<IPagedList<MaintenanceRequest>> SearchMaintenanceRequestsAsync(DateTime? createdFromUtc = null,
        DateTime? createdToUtc = null, int? statusId = null, bool? inWarranty = null, string fullName = null,
        string phoneNumber = null, string email = null, string city = null, string area = null, string brand = null,
        string deviceType = null, string modelNumber = null, string problem = null, string adminComment = null,
        string keywords = null, int pageIndex = 0, int pageSize = int.MaxValue);
}
