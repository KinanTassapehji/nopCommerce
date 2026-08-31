using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.CartTotal;

public class CartTotalDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, IPlugin, INopStationPlugin
{
	private readonly IActionContextAccessor _actionContextAccessor;

	private readonly IDiscountService _discountService;

	private readonly ILocalizationService _localizationService;

	private readonly IOrderTotalCalculationService _orderTotalCalculationService;

	private readonly ISettingService _settingService;

	private readonly IShoppingCartService _shoppingCartService;

	private readonly IUrlHelperFactory _urlHelperFactory;

	private readonly IWebHelper _webHelper;

	public CartTotalDiscountRequirementRule(IActionContextAccessor actionContextAccessor, IDiscountService discountService, ILocalizationService localizationService, IOrderTotalCalculationService orderTotalCalculationService, ISettingService settingService, IShoppingCartService shoppingCartService, IUrlHelperFactory urlHelperFactory, IWebHelper webHelper)
	{
		_actionContextAccessor = actionContextAccessor;
		_discountService = discountService;
		_localizationService = localizationService;
		_orderTotalCalculationService = orderTotalCalculationService;
		_settingService = settingService;
		_shoppingCartService = shoppingCartService;
		_urlHelperFactory = urlHelperFactory;
		_webHelper = webHelper;
	}

	public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		DiscountRequirementValidationResult result = new DiscountRequirementValidationResult
		{
			IsValid = false
		};
		decimal minimumCartTotal = await _settingService.GetSettingByKeyAsync(string.Format(DiscountRequirementDefaults.MinimumCartTotalSettingsKey, request.DiscountRequirementId), 0m);
		if (minimumCartTotal <= 0m)
		{
			return result;
		}
		if (request.Customer == null)
		{
			return result;
		}
		IList<ShoppingCartItem> cart = await _shoppingCartService.GetShoppingCartAsync(request.Customer, (ShoppingCartType?)ShoppingCartType.ShoppingCart, request.Store.Id, (int?)null, (DateTime?)null, (DateTime?)null);
		if (!cart.Any())
		{
			return result;
		}
		decimal num;
		if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
		{
			decimal item = (await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, includingTax: true)).Item4;
			num = ((!(item >= 0m)) ? 0m : item);
		}
		else
		{
			decimal? item2 = (await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart, false)).Item1;
			num = item2.GetValueOrDefault();
		}
		if (num >= minimumCartTotal)
		{
			result.IsValid = true;
			return result;
		}
		DiscountRequirementValidationResult discountRequirementValidationResult = result;
		discountRequirementValidationResult.UserError = await _localizationService.GetResourceAsync("NopStation.DiscountRules.CartTotal.InvalidForCartTotal");
		return result;
	}

	public string GetConfigurationUrl(int discountId, int? discountRequirementId)
	{
		return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).Action("Configure", "DiscountRulesCartTotal", new { discountId, discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
	}

	public override async Task InstallAsync()
	{
		await this.InstallPluginAsync();
		await base.InstallAsync();
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["Admin.NopStation.DiscountRules.CartTotal.Fields.MinimumCartTotal"] = "Minimum cart total",
			["Admin.NopStation.DiscountRules.CartTotal.Fields.MinimumCartTotal.Hint"] = "Discount will apply when cart total meets or exceeds this amount.",
			["Admin.NopStation.DiscountRules.CartTotal.Fields.MinimumCartTotal.Range"] = "Minimum cart total should be greater than zero.",
			["NopStation.DiscountRules.CartTotal.InvalidForCartTotal"] = "Sorry, this offer requires a higher cart total."
		};
	}

	public override async Task UninstallAsync()
	{
		IEnumerable<DiscountRequirement> enumerable = (await _discountService.GetAllDiscountRequirementsAsync()).Where((DiscountRequirement discountRequirement) => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SystemName);
		foreach (DiscountRequirement item in enumerable)
		{
			await _discountService.DeleteDiscountRequirementAsync(item, recursively: false);
		}
		await this.UninstallPluginAsync();
		await base.UninstallAsync();
	}
}
