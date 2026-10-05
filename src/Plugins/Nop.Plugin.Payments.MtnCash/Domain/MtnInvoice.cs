using Nop.Core;

namespace Nop.Plugin.Payments.MtnCash.Domain;
public class MtnInvoice : BaseEntity
{
    public string SessionNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTime Expired { get; set; }
    public DateTime Processed { get; set; }
    public string Description { get; set; }
    public ulong Pos { get; set; }
    public string QR { get; set; }
    public ulong Status { get; set; }
    public string Transaction { get; set; }
    public string MtnOperationNumber { get; set; }
    public string PaymentGuid { get; set; }
    public int? OrderId { get; set; }
}
