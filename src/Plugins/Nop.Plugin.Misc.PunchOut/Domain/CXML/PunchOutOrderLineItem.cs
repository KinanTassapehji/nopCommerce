namespace Nop.Plugin.Misc.PunchOut.Domain.CXML;

/// <summary>
/// Represents a PunchOut order line item
/// </summary>
public class PunchOutOrderLineItem
{
    public string SupplierPartId { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string CurrencyCode { get; set; }
    public string UnitOfMeasure { get; set; }
}
