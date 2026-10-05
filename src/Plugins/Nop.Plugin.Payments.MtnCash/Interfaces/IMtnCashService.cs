using Nop.Plugin.Payments.MtnCash.Domain;

namespace Nop.Plugin.Payments.MtnCash.Interfaces;
public interface IMtnCashService
{
    Task<MtnInvoice> CreateMtnInvoiceAsync(decimal amount, int orderId);
    Task InitMtnPayment(int orderId, string phoneNumber);
    Task ConifermMtnPayment(string phoneNumber, int orderId, string code);
}
