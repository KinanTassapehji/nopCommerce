using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Core.Domain.Directory;
using Nop.Services.Directory;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Widgets.ProductRibbon.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductRibbon.Infrastructure.Cache;

namespace NopStation.Plugin.Widgets.ProductRibbon.Areas.Admin.Controllers;

public class ProductRibbonController : NopStationAdminController
{
	private readonly ISettingHelper<ProductRibbonSettings, ConfigurationModel> _settingHelper;

	private readonly IBaseAdminModelFactory _baseAdminModelFactory;

	private readonly ICurrencyService _currencyService;

	private readonly CurrencySettings _currencySettings;

	private readonly IStaticCacheManager _cacheManager;

	public ProductRibbonController(ISettingHelper<ProductRibbonSettings, ConfigurationModel> settingHelper, IBaseAdminModelFactory baseAdminModelFactory, ICurrencyService currencyService, CurrencySettings currencySettings, IStaticCacheManager cacheManager)
	{
		_settingHelper = settingHelper;
		_baseAdminModelFactory = baseAdminModelFactory;
		_currencyService = currencyService;
		_currencySettings = currencySettings;
		_cacheManager = cacheManager;
	}

	[CheckPermission("ManageNopStationProductRibbon", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		ConfigurationModel model = await _settingHelper.PrepareConfigurationModelAsync(null);
		await _baseAdminModelFactory.PrepareOrderStatusesAsync(model.AvailableOrderStatuses, withSpecialDefaultItem: false);
		await _baseAdminModelFactory.PreparePaymentStatusesAsync(model.AvailablePaymentStatuses, withSpecialDefaultItem: false);
		await _baseAdminModelFactory.PrepareShippingStatusesAsync(model.AvailableShippingStatuses, withSpecialDefaultItem: false);
		model.CurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode;
		return View(model);
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationProductRibbon", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		await _settingHelper.SaveConfigurationModelAsync(model, null, true);
		await _cacheManager.RemoveByPrefixAsync(ProductRibbonCacheDefaults.PRODUCT_RIBBON_PATTERN_KEY);
		return RedirectToAction("Configure");
	}
}
