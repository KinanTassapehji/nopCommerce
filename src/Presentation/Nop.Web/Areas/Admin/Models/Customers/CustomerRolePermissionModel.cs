namespace Nop.Web.Areas.Admin.Models.Customers;

/// <summary>
/// Represents one permission on the customer role page: "Admin area. Orders. View" shows as
/// the action "View" in the section "Orders" of the category card "Orders"
/// </summary>
public partial record CustomerRolePermissionModel
{
    public int Id { get; set; }

    /// <summary>
    /// Permission category system name (e.g. "ContentManagement"); picks the card's icon
    /// </summary>
    public string Category { get; set; }

    public string CategoryName { get; set; }

    public string Section { get; set; }

    public string Action { get; set; }

    /// <summary>
    /// "View", "Edit" or "Import" for the three actions most sections share (they get a column each); null for the rest
    /// </summary>
    public string Column { get; set; }

    /// <summary>
    /// What the permission lets people do, shown as a tooltip; empty when none is written
    /// </summary>
    public string Description { get; set; }

    public bool Selected { get; set; }
}