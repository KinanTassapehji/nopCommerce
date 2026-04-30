namespace Nop.Plugin.Misc.PunchOut.Domain.CXML;

/// <summary>
/// Represents a PunchOut order response
/// </summary>
public class PunchOutOrderResponse : BasePunchOutModel
{
    public string StatusCode { get; set; }

    public string StatusText { get; set; }
}
