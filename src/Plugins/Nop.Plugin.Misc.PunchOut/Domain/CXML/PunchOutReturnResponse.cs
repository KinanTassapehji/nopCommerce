namespace Nop.Plugin.Misc.PunchOut.Domain.CXML;

/// <summary>
/// Represents a PunchOut return response
/// </summary>
public class PunchOutReturnResponse
{
    public string SessionId { get; set; }

    public string Html { get; set; }
}
