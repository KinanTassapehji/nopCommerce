using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Payments.CashOnDelivery.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.CashOnDelivery.Components;

public class PaymentCashOnDeliveryViewComponent : NopViewComponent
{
	private readonly ILocalizationService _localizationService;

	private readonly ISettingService _settingService;

	private readonly IStoreContext _storeContext;

	private readonly IWorkContext _workContext;

	public PaymentCashOnDeliveryViewComponent(ILocalizationService localizationService, ISettingService settingService, IStoreContext storeContext, IWorkContext workContext)
	{
		_localizationService = localizationService;
		_settingService = settingService;
		_storeContext = storeContext;
		_workContext = workContext;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Store currentStore = await _storeContext.GetCurrentStoreAsync();
		Language currentLanguage = await _workContext.GetWorkingLanguageAsync();
		CashOnDeliveryPaymentSettings cashOnDeliveryPaymentSettings = await _settingService.LoadSettingAsync<CashOnDeliveryPaymentSettings>(currentStore.Id);
		PaymentInfoModel paymentInfoModel = new PaymentInfoModel();
		PaymentInfoModel paymentInfoModel2 = paymentInfoModel;
		paymentInfoModel2.DescriptionText = await _localizationService.GetLocalizedSettingAsync(cashOnDeliveryPaymentSettings, (CashOnDeliveryPaymentSettings x) => x.DescriptionText, currentLanguage.Id, 0);
		PaymentInfoModel model = paymentInfoModel;
		return View("~/Plugins/Payments.CashOnDelivery/Views/PaymentInfo.cshtml", model);
	}
}
