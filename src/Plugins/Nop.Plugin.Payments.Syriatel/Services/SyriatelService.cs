using System.Globalization;
using Newtonsoft.Json.Linq;
using Nop.Plugin.Payments.Syriatel.Interfaces;
using Nop.Services.Configuration;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.Syriatel.Services;

internal class SyriatelService : ISyriatelService
{
    private readonly ISyriatelApiClient _apiClient;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IOrderService _orderService;
    private readonly ISettingService _settingService;
    private readonly ISyriatelTokenProvider _tokenProvider;

    public SyriatelService(
        ISyriatelApiClient apiClient,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        ISettingService settingService,
        ISyriatelTokenProvider tokenProvider)
    {
        _apiClient = apiClient;
        _orderProcessingService = orderProcessingService;
        _orderService = orderService;
        _settingService = settingService;
        _tokenProvider = tokenProvider;
    }

    public async Task RequestPaymentAsync(int orderId, string customerMsisdn, decimal amount)
    {
        var settings = await _settingService.LoadSettingAsync<SyriatelPaymentSettings>();
        var token = await _tokenProvider.GetTokenAsync();

        var requestBody = new
        {
            customerMSISDN = customerMsisdn,
            merchantMSISDN = settings.MerchantMsisdn,
            amount = ((int)amount).ToString(CultureInfo.InvariantCulture),
            transactionID = orderId.ToString(CultureInfo.InvariantCulture),
            token
        };

        var response = await _apiClient.PostAsync("paymentRequest", requestBody);
        EnsureSuccess(response, "paymentRequest");
    }

    public async Task ConfirmPaymentAsync(int orderId, string otp)
    {
        var settings = await _settingService.LoadSettingAsync<SyriatelPaymentSettings>();
        var token = await _tokenProvider.GetTokenAsync();

        var requestBody = new
        {
            OTP = otp,
            merchantMSISDN = settings.MerchantMsisdn,
            transactionID = orderId.ToString(CultureInfo.InvariantCulture),
            token
        };

        var response = await _apiClient.PostAsync("paymentConfirmation", requestBody);
        EnsureSuccess(response, "paymentConfirmation");

        var order = await _orderService.GetOrderByIdAsync(orderId)
            ?? throw new Exception("Order not found.");

        order.AuthorizationTransactionId = orderId.ToString(CultureInfo.InvariantCulture);
        await _orderService.UpdateOrderAsync(order);
        await _orderProcessingService.MarkOrderAsPaidAsync(order);
    }

    public async Task ResendOtpAsync(int orderId)
    {
        var settings = await _settingService.LoadSettingAsync<SyriatelPaymentSettings>();
        var token = await _tokenProvider.GetTokenAsync();

        var requestBody = new
        {
            merchantMSISDN = settings.MerchantMsisdn,
            transactionID = orderId.ToString(CultureInfo.InvariantCulture),
            token
        };

        var response = await _apiClient.PostAsync("resendOTP", requestBody);
        EnsureSuccess(response, "resendOTP");
    }

    private async Task<string> GetTokenAsync()
    {
        var settings = await _settingService.LoadSettingAsync<SyriatelPaymentSettings>();
        var requestBody = new
        {
            username = settings.Username,
            password = settings.Password
        };

        var response = await _apiClient.PostAsync("getToken", requestBody);
        var json = JObject.Parse(response);
        var errorCode = json.Value<int>("errorCode");

        if (errorCode != 0)
        {
            var errorDesc = json.Value<string>("errorDesc") ?? "Unknown error";
            throw new Exception($"Syriatel getToken failed ({errorCode}): {errorDesc}");
        }

        var token = json.Value<string>("token");
        if (string.IsNullOrWhiteSpace(token))
            throw new Exception("Syriatel getToken did not return a token.");

        return token;
    }

    private static void EnsureSuccess(string responseBody, string operationName)
    {
        var json = JObject.Parse(responseBody);
        var errorCode = json.Value<int>("errorCode");

        if (errorCode != 0)
        {
            var errorDesc = json.Value<string>("errorDesc") ?? "Unknown error";
            throw new Exception($"Syriatel {operationName} failed ({errorCode}): {errorDesc}");
        }
    }
}
