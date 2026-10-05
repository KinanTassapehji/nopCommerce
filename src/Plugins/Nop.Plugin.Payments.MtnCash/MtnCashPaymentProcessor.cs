using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.MtnCash.Components;
using Nop.Plugin.Payments.MtnCash.Interfaces;
using Nop.Plugin.Payments.MtnCash.Models;
using Nop.Plugin.Payments.MtnCash.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;


namespace Nop.Plugin.Payments.MtnCash;

/// <summary>
/// MtnCash payment processor
/// </summary>
public class MtnCashPaymentProcessor : BasePlugin, IPaymentMethod
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly MtnCashPaymentSettings _MtnCashPaymentSettings;
    private readonly IMtnCashService _mtnCashService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWorkContext _workContext;



    #endregion

    #region Ctor

    public MtnCashPaymentProcessor(ILocalizationService localizationService,
        IOrderTotalCalculationService orderTotalCalculationService,
        ISettingService settingService,
        IWebHelper webHelper,
        MtnCashPaymentSettings MtnCashPaymentSettings,
        IMtnCashService mtnCashService,
        IHttpContextAccessor httpContextAccessor,
        IWorkContext workContext)
    {
        _localizationService = localizationService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _MtnCashPaymentSettings = MtnCashPaymentSettings;
        _mtnCashService = mtnCashService;
        _httpContextAccessor = httpContextAccessor;
        _workContext = workContext;
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
        if (!request.CustomValues.TryGetValue("MtnPhone", out _))
            throw new NopException("MTN phone number is missing");

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
        if (customValues.TryGetValue("MtnPhone", out var stored))
        {
            var phone = stored.Value;
            if (string.IsNullOrEmpty(phone))
                throw new NopException("MTN phone number is missing");

            string error = null;
            try
            {
                await _mtnCashService.CreateMtnInvoiceAsync(
                    request.Order.OrderTotal,
                    request.Order.Id);
                await _mtnCashService.InitMtnPayment(
                    request.Order.Id,
                    phone);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            var url = $"{_webHelper.GetStoreLocation()}MtnCash/ConfirmPayment"
                + $"?orderId={request.Order.Id}&phone={Uri.EscapeDataString(phone)}"
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
            _MtnCashPaymentSettings.AdditionalFee, _MtnCashPaymentSettings.AdditionalFeePercentage);
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
            Phone = form["Phone"]
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
        var phone = form["Phone"].ToString();

        var request = new ProcessPaymentRequest();

        request.CustomValues["MtnPhone"] = phone;

        return Task.FromResult(request);
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PaymentMtnCash/Configure";
    }

    /// <summary>
    /// Gets a type of a view component for displaying plugin in public store ("payment info" checkout step)
    /// </summary>
    /// <returns>View component type</returns>
    public Type GetPublicViewComponent()
    {
        return typeof(PaymentMtnCashViewComponent);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //settings
        //terminal ids and RSA keys are per merchant and secret: entered on the
        //configure page (or carried over in the database), never shipped in source
        var settings = new MtnCashPaymentSettings();
        await _settingService.SaveSettingAsync(settings);

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Payments.MtnCash.Fields.AdditionalFee"] = "Additional fee",
            ["Plugins.Payments.MtnCash.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
            ["Plugins.Payments.MtnCash.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
            ["Plugins.Payments.MtnCash.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
            ["Plugins.Payments.MtnCash.PaymentMethodDescription"] = "Pay with MtnCash",
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<MtnCashPaymentSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.MtnCash");

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
        return await _localizationService.GetResourceAsync("Plugins.Payments.MtnCash.PaymentMethodDescription");
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