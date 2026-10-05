namespace Nop.Plugin.Payments.Syriatel.Interfaces;

public interface ISyriatelService
{
    Task RequestPaymentAsync(int orderId, string customerMsisdn, decimal amount);
    Task ConfirmPaymentAsync(int orderId, string otp);
    Task ResendOtpAsync(int orderId);
}
