using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Data;
using Nop.Plugin.Payments.MtnCash.Domain;
using Nop.Plugin.Payments.MtnCash.Interfaces;
using Nop.Plugin.Payments.MtnCash.Services.Response;
using Nop.Services.Localization;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.MtnCash.Services;

internal class MtnCashService : IMtnCashService
{
    private readonly IRepository<MtnInvoice> _repository;
    private readonly ILocalizationService _l;
    private readonly IMtnApiClient _mtnApiClient;
    private readonly IOrderService _orderService;
    private readonly IOrderProcessingService _orderProcessingService;

    public MtnCashService(
        IRepository<MtnInvoice> repository,
        ILocalizationService l,
        IMtnApiClient mtnApiClient,
        IOrderService orderService,
        IOrderProcessingService orderProcessingService)
    {
        _repository = repository;
        _l = l;
        _mtnApiClient = mtnApiClient;
        _orderService = orderService;
        _orderProcessingService = orderProcessingService;
    }

    public async Task InitMtnPayment(int orderId, string phoneNumber)
    {
        phoneNumber = NormalizePhone(phoneNumber);

        // If payment was already initiated for this order, skip re-initiation
        var invoice = await _repository
            .Table
            .FirstOrDefaultAsync(x => x.OrderId == orderId);

        var guid = Guid.NewGuid().ToString();

        var requestBody = new
        {
            Invoice = orderId,
            Phone = phoneNumber,
            Guid = guid
        };

        var response = await _mtnApiClient.PostAsync(
            "pos_web/payment_phone/initiate",
            requestBody
        );

        JObject json = JObject.Parse(response);

        if ((int)json["Errno"] != 0)
            throw new Exception($"{json["Error"]}");

        string operationNumber = (string)json["OperationNumber"];

        if (!ulong.TryParse(operationNumber, out _))
            throw new Exception("Invalid operation number format.");

        if (invoice == null)
            throw new Exception("Invoice not found. Please place the order again.");

        invoice.PaymentGuid = guid;
        invoice.MtnOperationNumber = operationNumber;

        await _repository.UpdateAsync(invoice);
    }

    public async Task ConifermMtnPayment(string phoneNumber, int orderId, string code)
    {
        phoneNumber = NormalizePhone(phoneNumber);

        var invoice = await _repository
            .Table
            .FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (invoice == null)
            throw new Exception("Invoice not found.");
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            throw new Exception("Order not found.");

        if (!ulong.TryParse(invoice.MtnOperationNumber, out ulong operationNumber))
            throw new Exception("Invalid operation number format.");

        var requestBody = new
        {
            Invoice = orderId,
            Phone = phoneNumber,
            Guid = invoice.PaymentGuid,
            Code = HashCode(code),
            OperationNumber = operationNumber
        };

        string response = await _mtnApiClient.PostAsync(
            "pos_web/payment_phone/confirm",
            requestBody
        );

        JObject json = JObject.Parse(response);

        if ((int)json["Errno"] != 0)
            throw new Exception($"{json["Error"]}");

        invoice.Transaction = (string)json["Transaction"];
        invoice.Status = 9;

        await _repository.UpdateAsync(invoice);
        order.AuthorizationTransactionId = invoice.Transaction;
        await _orderService.UpdateOrderAsync(order);
        await _orderProcessingService.MarkOrderAsPaidAsync(order);

        //Order order = await _repository
        //    .FirstOrDefaultAsync<Order>(x => x.Id == orderId);

        //order.PaymentStatus = PaymentStatus.Completed;
        //order.OrderStatus = OrderStatus.Paid;

        //await _repository.UpdateAsync(order);
        //await _repository.SaveChangesAsync();
    }

    public async Task<MtnInvoice> CreateMtnInvoiceAsync(decimal amount, int orderId)
    {
        var exisitingInvoice = await _repository.Table.FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (exisitingInvoice != null)
        {
            return exisitingInvoice;
        }
        int newAmount = (int)(amount * 100);

        var requestBody = new
        {
            Amount = newAmount,
            Invoice = orderId,
            TTL = 4320
        };

        string response = await _mtnApiClient.PostAsync(
            "pos_web/invoice/create",
            requestBody
        );

        var json = JObject.Parse(response);

        int errno = (int)json["Errno"];

        // If invoice already exists on MTN side but not locally, create a local record
        if (errno != 0)
        {
            if (json["Receipt"] == null || json["Receipt"].Type == JTokenType.Null)
            {
                // Already exists on MTN but no receipt returned — create a minimal local record
                var minimalInvoice = new MtnInvoice
                {
                    OrderId = orderId,
                    Amount = newAmount
                };
                await _repository.InsertAsync(minimalInvoice);
                return minimalInvoice;
            }
        }

        InvoiceResponse receipt = JsonConvert.DeserializeObject<InvoiceResponse>(
            json["Receipt"].ToString()
        );
        var invoice = new MtnInvoice
        {
            OrderId = orderId,
            Amount = receipt.Amount,
            SessionNumber = receipt.SessionNumber,
            Currency = receipt.Currency,
            Description = receipt.Description,
            Expired = InvoiceResponse.UnixTimeStampToDateTime(receipt.Expired),
            Processed = InvoiceResponse.UnixTimeStampToDateTime(receipt.Processed),
            QR = receipt.QR,
            Pos = receipt.Pos,
            Status = receipt.Status,
            Transaction = receipt.Transaction
        };
        await _repository.InsertAsync(invoice);

        return invoice;
    }

    #region Helpers

    private static string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new Exception("Phone number is required.");

        phone = phone[1..];
        return "963" + phone;
    }

    private static string HashCode(string code)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(hash);
    }

    #endregion
}