namespace Nop.Plugin.Misc.PunchOut.Domain.CXML;

/// <summary>
/// Represents a PunchOut order request
/// </summary>
public class PunchOutOrderRequest : BasePunchOutModel
{
    public PunchOutOrderRequest()
    {
        LineItems = new List<PunchOutOrderLineItem>();
    }    

    public string Identity { get; set; }
    public string SharedSecret { get; set; }

    /// <summary>
    /// Contact information (Customer email)
    /// </summary>
    public string Contact { get; set; }

    /// <summary>
    /// The unique identifier for the order (OrderID)
    /// </summary>
    public string OrderID { get; set; }

    /// <summary>
    /// Billing address (BillTo)
    /// </summary>
    public PunchOutAddress BillTo { get; set; }

    /// <summary>
    /// Shipping address (ShipTo)
    /// </summary>
    public PunchOutAddress ShipTo { get; set; }

    /// <summary>
    /// Order line items
    /// </summary>
    public List<PunchOutOrderLineItem> LineItems { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string CurrencyCode { get; set; }

    /// <summary>
    /// Order total
    /// </summary>
    public decimal Total { get; set; }
}
