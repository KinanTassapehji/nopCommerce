using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.CashOnDelivery.Components;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.CashOnDelivery;

public class CashOnDeliveryPaymentProcessor : BasePlugin, IPaymentMethod, IPlugin
{
	private readonly CashOnDeliveryPaymentSettings _cashOnDeliveryPaymentSettings;

	private readonly ILocalizationService _localizationService;

	private readonly IOrderTotalCalculationService _orderTotalCalculationService;

	private readonly ISettingService _settingService;

	private readonly IShoppingCartService _shoppingCartService;

	private readonly IWebHelper _webHelper;

	public bool SupportCapture => false;

	public bool SupportPartiallyRefund => false;

	public bool SupportRefund => false;

	public bool SupportVoid => false;

	public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

	public bool SkipPaymentInfo => _cashOnDeliveryPaymentSettings.SkipPaymentInfo;

	public CashOnDeliveryPaymentProcessor(CashOnDeliveryPaymentSettings cashOnDeliveryPaymentSettings, ILocalizationService localizationService, IOrderTotalCalculationService orderTotalCalculationService, ISettingService settingService, IShoppingCartService shoppingCartService, IWebHelper webHelper)
	{
		_cashOnDeliveryPaymentSettings = cashOnDeliveryPaymentSettings;
		_localizationService = localizationService;
		_orderTotalCalculationService = orderTotalCalculationService;
		_settingService = settingService;
		_shoppingCartService = shoppingCartService;
		_webHelper = webHelper;
	}

	public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
	{
		return Task.FromResult(new ProcessPaymentResult
		{
			NewPaymentStatus = PaymentStatus.Pending
		});
	}

	public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
	{
		return Task.CompletedTask;
	}

	public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
	{
		return false;
	}

	public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
	{
		return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart, _cashOnDeliveryPaymentSettings.AdditionalFee, _cashOnDeliveryPaymentSettings.AdditionalFeePercentage);
	}

	public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
	{
		CapturePaymentResult capturePaymentResult = new CapturePaymentResult();
		capturePaymentResult.Errors = new string[1] { "Capture method not supported" };
		return Task.FromResult(capturePaymentResult);
	}

	public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
	{
		RefundPaymentResult refundPaymentResult = new RefundPaymentResult();
		refundPaymentResult.Errors = new string[1] { "Refund method not supported" };
		return Task.FromResult(refundPaymentResult);
	}

	public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
	{
		VoidPaymentResult voidPaymentResult = new VoidPaymentResult();
		voidPaymentResult.Errors = new string[1] { "Void method not supported" };
		return Task.FromResult(voidPaymentResult);
	}

	public Task<bool> CanRePostProcessPaymentAsync(Order order)
	{
		ArgumentNullException.ThrowIfNull(order, "order");
		return Task.FromResult(result: false);
	}

	public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
	{
		return Task.FromResult((IList<string>)new List<string>());
	}

	public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
	{
		return Task.FromResult(new ProcessPaymentRequest());
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/PaymentCashOnDelivery/Configure";
	}

	public override async Task InstallAsync()
	{
		CashOnDeliveryPaymentSettings settings = new CashOnDeliveryPaymentSettings
		{
			DescriptionText = "<p>In cases where an order is placed, an authorized representative will contact you, personally or over telephone, to confirm the order.<br />After the order is confirmed, it will be processed.<br />Orders once confirmed, cannot be cancelled.</p><p>P.S. You can edit this text from admin panel.</p>",
			SkipPaymentInfo = false
		};
		await _settingService.SaveSettingAsync(settings);
		await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
		{
			["Plugins.Payment.CashOnDelivery.DescriptionText"] = "Description",
			["Plugins.Payment.CashOnDelivery.DescriptionText.Hint"] = "Enter info that will be shown to customers during checkout",
			["Plugins.Payment.CashOnDelivery.AdditionalFee"] = "Additional fee",
			["Plugins.Payment.CashOnDelivery.AdditionalFee.Hint"] = "The additional fee.",
			["Plugins.Payment.CashOnDelivery.AdditionalFeePercentage"] = "Additional fee. Use percentage",
			["Plugins.Payment.CashOnDelivery.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
			["Plugins.Payment.CashOnDelivery.ShippableProductRequired"] = "Shippable product required",
			["Plugins.Payment.CashOnDelivery.ShippableProductRequired.Hint"] = "An option indicating whether shippable products are required in order to display this payment method during checkout.",
			["Plugins.Payment.CashOnDelivery.PaymentMethodDescription"] = "Pay by \"Cash on delivery\"",
			["Plugins.Payment.CashOnDelivery.SkipPaymentInfo"] = "Skip payment information page",
			["Plugins.Payment.CashOnDelivery.SkipPaymentInfo.Hint"] = "An option indicating whether we should display a payment information page for this plugin."
		});
		await base.InstallAsync();
	}

	public override async Task UninstallAsync()
	{
		await _settingService.DeleteSettingAsync<CashOnDeliveryPaymentSettings>();
		await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payment.CashOnDelivery");
		await base.UninstallAsync();
	}

	public Type GetPublicViewComponent()
	{
		return typeof(PaymentCashOnDeliveryViewComponent);
	}

	public async Task<string> GetPaymentMethodDescriptionAsync()
	{
		return await _localizationService.GetResourceAsync("Plugins.Payment.CashOnDelivery.PaymentMethodDescription");
	}
}
