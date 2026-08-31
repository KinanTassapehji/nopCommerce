using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.CashOnDelivery.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.CashOnDelivery.Controllers;

[AuthorizeAdmin(false)]
[Area("Admin")]
[AutoValidateAntiforgeryToken]
public class PaymentCashOnDeliveryController : BasePaymentController
{
	private readonly ILanguageService _languageService;

	private readonly ILocalizationService _localizationService;

	private readonly INotificationService _notificationService;

	private readonly ISettingService _settingService;

	private readonly IStoreContext _storeContext;

	public PaymentCashOnDeliveryController(ILanguageService languageService, ILocalizationService localizationService, INotificationService notificationService, ISettingService settingService, IStoreContext storeContext)
	{
		_languageService = languageService;
		_localizationService = localizationService;
		_notificationService = notificationService;
		_settingService = settingService;
		_storeContext = storeContext;
	}

	[CheckPermission("Configuration.ManagePaymentMethods", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
		CashOnDeliveryPaymentSettings cashOnDeliveryPaymentSettings = await _settingService.LoadSettingAsync<CashOnDeliveryPaymentSettings>(storeScope);
		ConfigurationModel model = new ConfigurationModel
		{
			DescriptionText = cashOnDeliveryPaymentSettings.DescriptionText
		};
		await AddLocalesAsync(_languageService, model.Locales, async delegate(ConfigurationLocalizedModel locale, int languageId)
		{
			locale.DescriptionText = await _localizationService.GetLocalizedSettingAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.DescriptionText, languageId, 0, returnDefaultValue: false, ensureTwoPublishedLanguages: false);
		});
		model.AdditionalFee = cashOnDeliveryPaymentSettings.AdditionalFee;
		model.AdditionalFeePercentage = cashOnDeliveryPaymentSettings.AdditionalFeePercentage;
		model.ShippableProductRequired = cashOnDeliveryPaymentSettings.ShippableProductRequired;
		model.SkipPaymentInfo = cashOnDeliveryPaymentSettings.SkipPaymentInfo;
		model.ActiveStoreScopeConfiguration = storeScope;
		if (storeScope > 0)
		{
			ConfigurationModel configurationModel = model;
			configurationModel.DescriptionText_OverrideForStore = await _settingService.SettingExistsAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.DescriptionText, storeScope);
			ConfigurationModel configurationModel2 = model;
			configurationModel2.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.AdditionalFee, storeScope);
			ConfigurationModel configurationModel3 = model;
			configurationModel3.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.AdditionalFeePercentage, storeScope);
			ConfigurationModel configurationModel4 = model;
			configurationModel4.ShippableProductRequired_OverrideForStore = await _settingService.SettingExistsAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.ShippableProductRequired, storeScope);
			ConfigurationModel configurationModel5 = model;
			configurationModel5.SkipPaymentInfo_OverrideForStore = await _settingService.SettingExistsAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.SkipPaymentInfo, storeScope);
		}
		return View("~/Plugins/Payments.CashOnDelivery/Views/Configure.cshtml", model);
	}

	[HttpPost]
	[CheckPermission("Configuration.ManagePaymentMethods", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		if (!base.ModelState.IsValid)
		{
			return await Configure();
		}
		int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
		CashOnDeliveryPaymentSettings cashOnDeliveryPaymentSettings = await _settingService.LoadSettingAsync<CashOnDeliveryPaymentSettings>(storeScope);
		cashOnDeliveryPaymentSettings.DescriptionText = model.DescriptionText;
		cashOnDeliveryPaymentSettings.AdditionalFee = model.AdditionalFee;
		cashOnDeliveryPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
		cashOnDeliveryPaymentSettings.ShippableProductRequired = model.ShippableProductRequired;
		cashOnDeliveryPaymentSettings.SkipPaymentInfo = model.SkipPaymentInfo;
		await _settingService.SaveSettingOverridablePerStoreAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.ShippableProductRequired, model.ShippableProductRequired_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.SkipPaymentInfo, model.SkipPaymentInfo_OverrideForStore, storeScope, clearCache: false);
		await _settingService.ClearCacheAsync();
		foreach (ConfigurationLocalizedModel localized in model.Locales)
		{
			await _localizationService.SaveLocalizedSettingAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.DescriptionText, localized.LanguageId, localized.DescriptionText);
		}
		INotificationService notificationService = _notificationService;
		notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
		return await Configure();
	}
}
