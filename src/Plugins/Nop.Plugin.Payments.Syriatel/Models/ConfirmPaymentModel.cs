namespace Nop.Plugin.Payments.Syriatel.Models;
public class ConfirmPaymentModel
{
    public string CustomerMsisdn { get; set; }
    public string Otp { get; set; }
    public int OrderId { get; set; }
    public string ErrorMessage { get; set; }
    public bool InitFailed { get; set; }
}
