using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Syriatel.Components;
using Nop.Plugin.Payments.Syriatel.Interfaces;
using Nop.Plugin.Payments.Syriatel.Models;
using Nop.Plugin.Payments.Syriatel.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;


namespace Nop.Plugin.Payments.Syriatel;

/// <summary>
/// Syriatel payment processor
/// </summary>
public class SyriatelPaymentProcessor : BasePlugin, IPaymentMethod
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly SyriatelPaymentSettings _syriatelPaymentSettings;
    private readonly ISyriatelService _syriatelService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWorkContext _workContext;
    private readonly ILanguageService _languageService;



    #endregion

    #region Ctor

    public SyriatelPaymentProcessor(ILocalizationService localizationService,
        IOrderTotalCalculationService orderTotalCalculationService,
        ISettingService settingService,
        IWebHelper webHelper,
        SyriatelPaymentSettings syriatelPaymentSettings,
        ISyriatelService syriatelService,
        IHttpContextAccessor httpContextAccessor,
        IWorkContext workContext,
        ILanguageService languageService)
    {
        _localizationService = localizationService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _syriatelPaymentSettings = syriatelPaymentSettings;
        _syriatelService = syriatelService;
        _httpContextAccessor = httpContextAccessor;
        _workContext = workContext;
        _languageService = languageService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Process a payment
    /// </summary>
    /// <param name="processPaymentRequest">Payment info required for an order processing</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the process payment result
    /// </returns>
    public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        if (!request.CustomValues.TryGetValue("SyriatelCustomerMsisdn", out _))
            throw new NopException("Customer MSISDN is missing");

        return new ProcessPaymentResult
        {
            NewPaymentStatus = PaymentStatus.Pending
        };
    }

    /// <summary>
    /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
    /// </summary>
    /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task PostProcessPaymentAsync(PostProcessPaymentRequest request)
    {
        var customValues = new CustomValues();
        customValues.FillByXml(request.Order.CustomValuesXml);
        if (customValues.TryGetValue("SyriatelCustomerMsisdn", out var stored))
        {
            var customerMsisdn = stored.Value;
            if (string.IsNullOrEmpty(customerMsisdn))
                throw new NopException("Customer MSISDN is missing");

            string error = null;
            try
            {
                await _syriatelService.RequestPaymentAsync(
                    request.Order.Id,
                    customerMsisdn,
                    request.Order.OrderTotal);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            var url = $"{_webHelper.GetStoreLocation()}Syriatel/ConfirmPayment"
                + $"?orderId={request.Order.Id}&customerMsisdn={Uri.EscapeDataString(customerMsisdn)}"
                + (error != null ? $"&error={Uri.EscapeDataString(error)}" : "");

            _httpContextAccessor.HttpContext.Response.Redirect(url);
        }
    }


    /// <summary>
    /// Returns a value indicating whether payment method should be hidden during checkout
    /// </summary>
    /// <param name="cart">Shopping cart</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains true - hide; false - display.
    /// </returns>
    public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        var currency = await _workContext.GetWorkingCurrencyAsync();

        // Hide if currency is not SYP
        return currency == null
            || !string.Equals(currency.CurrencyCode, "SYP", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets additional handling fee
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the additional handling fee
    /// </returns>
    public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
            _syriatelPaymentSettings.AdditionalFee, _syriatelPaymentSettings.AdditionalFeePercentage);
    }

    /// <summary>
    /// Captures payment
    /// </summary>
    /// <param name="capturePaymentRequest">Capture payment request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the capture payment result
    /// </returns>
    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
    }

    /// <summary>
    /// Refunds a payment
    /// </summary>
    /// <param name="refundPaymentRequest">Request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
    }

    /// <summary>
    /// Voids a payment
    /// </summary>
    /// <param name="voidPaymentRequest">Request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
    }



    /// <summary>
    /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<bool> CanRePostProcessPaymentAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return Task.FromResult(order.PaymentStatus == PaymentStatus.Pending);
    }

    /// <summary>
    /// Validate payment form
    /// </summary>
    /// <param name="form">The parsed form values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of validating errors
    /// </returns>
    public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        var warnings = new List<string>();

        var model = new PaymentInfoModel
        {
            CustomerMsisdn = form["CustomerMsisdn"]
        };

        var validator = new PaymentInfoValidator(_localizationService);
        var validationResult = validator.Validate(model);

        if (!validationResult.IsValid)
            warnings.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));

        return Task.FromResult<IList<string>>(warnings);
    }

    /// <summary>
    /// Get payment information
    /// </summary>
    /// <param name="form">The parsed form values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the payment info holder
    /// </returns>
    public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        var customerMsisdn = form["CustomerMsisdn"].ToString();

        var request = new ProcessPaymentRequest();

        request.CustomValues["SyriatelCustomerMsisdn"] = customerMsisdn;

        return Task.FromResult(request);
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PaymentSyriatel/Configure";
    }

    /// <summary>
    /// Gets a type of a view component for displaying plugin in public store ("payment info" checkout step)
    /// </summary>
    /// <returns>View component type</returns>
    public Type GetPublicViewComponent()
    {
        return typeof(PaymentSyriatelViewComponent);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //settings
        var settings = new SyriatelPaymentSettings
        {
            BaseUrl = SyriatelConsts.DefaultBaseUrl
        };
        await _settingService.SaveSettingAsync(settings);

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Payments.Syriatel.Fields.AdditionalFee"] = "Additional fee",
            ["Plugins.Payments.Syriatel.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
            ["Plugins.Payments.Syriatel.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
            ["Plugins.Payments.Syriatel.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
            ["Plugins.Payments.Syriatel.Fields.BaseUrl"] = "Base URL",
            ["Plugins.Payments.Syriatel.Fields.BaseUrl.Hint"] = "Base URL for Syriatel's API endpoints.",
            ["Plugins.Payments.Syriatel.Fields.Username"] = "Username",
            ["Plugins.Payments.Syriatel.Fields.Username.Hint"] = "Merchant username provided by Syriatel.",
            ["Plugins.Payments.Syriatel.Fields.Password"] = "Password",
            ["Plugins.Payments.Syriatel.Fields.Password.Hint"] = "Merchant password provided by Syriatel.",
            ["Plugins.Payments.Syriatel.Fields.MerchantMsisdn"] = "Merchant MSISDN",
            ["Plugins.Payments.Syriatel.Fields.MerchantMsisdn.Hint"] = "Merchant MSISDN registered with Syriatel.",
            ["Plugins.Payments.Syriatel.Instructions"] = "Configure the Syriatel credentials and base URL provided by Syriatel before enabling this payment method.",
            ["Plugins.Payments.Syriatel.PaymentMethodDescription"] = "Pay with Syriatel Cash",
            ["Plugins.Payments.Syriatel.Ui.ConfirmTitle"] = "Confirm Syriatel Cash payment",
            ["Plugins.Payments.Syriatel.Ui.CheckPhone"] = "Please check your phone number and try again",
            ["Plugins.Payments.Syriatel.Ui.Phone"] = "Phone:",
            ["Plugins.Payments.Syriatel.Ui.Retry"] = "Retry",
            ["Plugins.Payments.Syriatel.Ui.EnterCode"] = "Enter the code sent to your phone",
            ["Plugins.Payments.Syriatel.Ui.Code"] = "OTP:",
            ["Plugins.Payments.Syriatel.Ui.Confirm"] = "Confirm payment",
            ["Plugins.Payments.Syriatel.Ui.ChangePhone"] = "Change phone number",
            ["Plugins.Payments.Syriatel.Ui.ResendCode"] = "Resend code",
            ["Plugins.Payments.Syriatel.Ui.EnterAccount"] = "Enter your Syriatel Cash number",
            ["Plugins.Payments.Syriatel.Ui.Failed"] = "Payment failed",
            ["Plugins.Payments.Syriatel.Ui.FailedText"] = "We couldn't complete your Syriatel Cash payment.<br />No charges were made. You can retry payment from your order details.",
            ["Plugins.Payments.Syriatel.Ui.RetryPayment"] = "Retry payment",
            ["Plugins.Payments.Syriatel.Ui.MyOrders"] = "My orders",
        });

        //the store is Arabic-first: the customer-facing texts in every Arabic language
        foreach (var language in (await _languageService.GetAllLanguagesAsync(showHidden: true))
            .Where(l => l.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase)))
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.Syriatel.PaymentMethodDescription"] = "الدفع عبر Syriatel Cash",
                ["Plugins.Payments.Syriatel.Ui.ConfirmTitle"] = "تأكيد الدفع عبر Syriatel Cash",
                ["Plugins.Payments.Syriatel.Ui.CheckPhone"] = "يرجى التحقق من رقم الهاتف والمحاولة مرة أخرى",
                ["Plugins.Payments.Syriatel.Ui.Phone"] = "رقم الهاتف:",
                ["Plugins.Payments.Syriatel.Ui.Retry"] = "إعادة المحاولة",
                ["Plugins.Payments.Syriatel.Ui.EnterCode"] = "أدخل رمز التحقق المرسل إلى هاتفك",
                ["Plugins.Payments.Syriatel.Ui.Code"] = "رمز التحقق:",
                ["Plugins.Payments.Syriatel.Ui.Confirm"] = "تأكيد الدفع",
                ["Plugins.Payments.Syriatel.Ui.ChangePhone"] = "تغيير رقم الهاتف",
                ["Plugins.Payments.Syriatel.Ui.ResendCode"] = "إعادة إرسال الرمز",
                ["Plugins.Payments.Syriatel.Ui.EnterAccount"] = "أدخل رقم حساب Syriatel Cash",
                ["Plugins.Payments.Syriatel.Ui.Failed"] = "تعذّر الدفع",
                ["Plugins.Payments.Syriatel.Ui.FailedText"] = "تعذّر إتمام الدفع عبر Syriatel Cash.<br />لم يُخصم أي مبلغ، ويمكنك إعادة محاولة الدفع من صفحة تفاصيل الطلب.",
                ["Plugins.Payments.Syriatel.Ui.RetryPayment"] = "إعادة محاولة الدفع",
                ["Plugins.Payments.Syriatel.Ui.MyOrders"] = "طلباتي",
            }, language.Id);

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<SyriatelPaymentSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.Syriatel");

        await base.UninstallAsync();
    }

    /// <summary>
    /// Gets a payment method description that will be displayed on checkout pages in the public store
    /// </summary>
    /// <remarks>
    /// return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
    /// for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
    /// </remarks>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payments.Syriatel.PaymentMethodDescription");
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether capture is supported
    /// </summary>
    public bool SupportCapture => false;

    /// <summary>
    /// Gets a value indicating whether partial refund is supported
    /// </summary>
    public bool SupportPartiallyRefund => false;

    /// <summary>
    /// Gets a value indicating whether refund is supported
    /// </summary>
    public bool SupportRefund => false;

    /// <summary>
    /// Gets a value indicating whether void is supported
    /// </summary>
    public bool SupportVoid => false;


    /// <summary>
    /// Gets a payment method type
    /// </summary>
    public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

    /// <summary>
    /// Gets a value indicating whether we should display a payment information page for this plugin
    /// </summary>
    public bool SkipPaymentInfo => false;

    #endregion
}
