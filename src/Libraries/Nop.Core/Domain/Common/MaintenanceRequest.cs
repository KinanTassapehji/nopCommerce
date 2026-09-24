namespace Nop.Core.Domain.Common;

/// <summary>
/// Represents a maintenance request sent from the public "maintenance request" form
/// </summary>
public partial class MaintenanceRequest : BaseEntity
{
    /// <summary>
    /// Gets or sets the full name
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets the mobile number
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the email (the form leaves it optional)
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the city
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// Gets or sets the street
    /// </summary>
    public string Area { get; set; }

    /// <summary>
    /// Gets or sets the device brand
    /// </summary>
    public string Brand { get; set; }

    /// <summary>
    /// Gets or sets the device type
    /// </summary>
    public string DeviceType { get; set; }

    /// <summary>
    /// Gets or sets the model number
    /// </summary>
    public string ModelNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the device is within the warranty period
    /// </summary>
    public bool InWarranty { get; set; }

    /// <summary>
    /// Gets or sets the problem as the customer described it
    /// </summary>
    public string Problem { get; set; }

    /// <summary>
    /// Gets or sets the status identifier
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the note the staff keeps on this request
    /// </summary>
    public string AdminComment { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the request creation
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    #region Custom properties

    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public MaintenanceRequestStatus Status
    {
        get => (MaintenanceRequestStatus)StatusId;
        set => StatusId = (int)value;
    }

    #endregion
}