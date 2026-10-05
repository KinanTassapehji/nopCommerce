namespace Nop.Plugin.Payments.MtnCash.Services.Response;
public class InvoiceResponse
{
    public int Errno { get; set; }
    public string Error { get; set; }
    public int InvoiceNumber { get; set; }
    public string SessionNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public long Created { get; set; }
    public long Expired { get; set; }
    public long Processed { get; set; }
    public string Type { get; set; }
    public string SystemIssuer { get; set; }
    public string Description { get; set; }
    public ulong Pos { get; set; }
    public string QR { get; set; }
    public ulong Status { get; set; }
    public string Transaction { get; set; }
    public ulong Commission { get; set; }
    public ulong TaxNumber { get; set; }
    public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        System.DateTime dtDateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }
}

public enum InvoiceStatus
{
    Canceled = 0,
    Active = 1,
    Processing = 5,
    Success = 9,
    Fail = 8,
}
