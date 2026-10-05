namespace Nop.Plugin.Payments.MtnCash.Models;
public class ConfirmPaymentModel
{
    public string Phone { get; set; }
    public string Code { get; set; }
    public int OrderId { get; set; }
    public string ErrorMessage { get; set; }
    public bool InitFailed { get; set; }
}
