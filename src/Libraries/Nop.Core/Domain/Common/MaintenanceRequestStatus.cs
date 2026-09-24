namespace Nop.Core.Domain.Common;

/// <summary>
/// Represents a maintenance request status
/// </summary>
public enum MaintenanceRequestStatus
{
    /// <summary>
    /// Received, nobody has looked at it yet
    /// </summary>
    New = 10,

    /// <summary>
    /// A technician is on it
    /// </summary>
    InProgress = 20,

    /// <summary>
    /// The device was serviced
    /// </summary>
    Completed = 30,

    /// <summary>
    /// Dropped - the customer cancelled, or it was never a real request
    /// </summary>
    Cancelled = 40
}