using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Tax;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.OrderRange;

public class OrderRangeDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, IPlugin, INopStationPlugin
{
	private readonly IActionContextAccessor _actionContextAccessor;

	private readonly IUrlHelperFactory _urlHelperFactory;

	private readonly IShoppingCartService _shoppingCartService;

	private readonly IStoreContext _storeContext;

	private readonly IWorkContext _workContext;

	private readonly TaxSettings _taxSettings;

	private readonly IWebHelper _webHelper;

	private readonly IDiscountService _discountService;

	private readonly ISettingService _settingService;

	private readonly ICustomerService _customerService;

	private readonly IProductService _productService;

	private readonly ITaxService _taxService;

	public OrderRangeDiscountRequirementRule(IShoppingCartService shoppingCartService, IStoreContext storeContext, IWorkContext workContext, TaxSettings taxSettings, IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory, IWebHelper webHelper, IDiscountService discountService, ISettingService settingService, ICustomerService customerService, IProductService productService, ITaxService taxService)
	{
		_shoppingCartService = shoppingCartService;
		_storeContext = storeContext;
		_workContext = workContext;
		_taxSettings = taxSettings;
		_actionContextAccessor = actionContextAccessor;
		_urlHelperFactory = urlHelperFactory;
		_webHelper = webHelper;
		_discountService = discountService;
		_settingService = settingService;
		_customerService = customerService;
		_productService = productService;
		_taxService = taxService;
	}

	public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
	{
		Store store = await _storeContext.GetCurrentStoreAsync();
		IShoppingCartService shoppingCartService = _shoppingCartService;
		IList<ShoppingCartItem> cart = await shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), (ShoppingCartType?)ShoppingCartType.ShoppingCart, store.Id, (int?)null, (DateTime?)null, (DateTime?)null);
		DiscountRequirementValidationResult result = new DiscountRequirementValidationResult
		{
			IsValid = false
		};
		string conditionvalue = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.ConditionValueSettingsKey, request.DiscountRequirementId));
		int rangeValue = await _settingService.GetSettingByKeyAsync(string.Format(DiscountRequirementDefaults.RangeValueSettingsKey, request.DiscountRequirementId), 0);
		if (conditionvalue == null || conditionvalue == "0")
		{
			return result;
		}
		if (rangeValue == 0)
		{
			return result;
		}
		if (cart.Any())
		{
			SortedDictionary<decimal, decimal> taxRates = new SortedDictionary<decimal, decimal>();
			Customer customer = await _customerService.GetShoppingCartCustomerAsync(cart);
			decimal subTotalExclTaxWithoutDiscount = 0m;
			decimal subTotalInclTaxWithoutDiscount = 0m;
			foreach (ShoppingCartItem shoppingCartItem in cart)
			{
				decimal sciSubTotal = (await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, includeDiscounts: true)).Item1;
				Product product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);
				(decimal, decimal) tuple = await _taxService.GetProductPriceAsync(product, sciSubTotal, includingTax: false, customer);
				decimal sciExclTax = tuple.Item1;
				decimal taxRate = tuple.Item2;
				decimal item = (await _taxService.GetProductPriceAsync(product, sciSubTotal, includingTax: true, customer)).Item1;
				subTotalExclTaxWithoutDiscount += sciExclTax;
				subTotalInclTaxWithoutDiscount += item;
				decimal num = item - sciExclTax;
				if (!(taxRate <= 0m) && !(num <= 0m))
				{
					if (!taxRates.ContainsKey(taxRate))
					{
						taxRates.Add(taxRate, num);
					}
					else
					{
						taxRates[taxRate] += num;
					}
				}
			}
			bool num2 = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
			decimal num3 = subTotalExclTaxWithoutDiscount;
			if (num2)
			{
				num3 = subTotalInclTaxWithoutDiscount;
			}
			if ((conditionvalue == "G" && num3 > (decimal)rangeValue) || (conditionvalue == "L" && num3 < (decimal)rangeValue) || (conditionvalue == "E" && num3 == (decimal)rangeValue))
			{
				result.IsValid = true;
			}
		}
		return result;
	}

	public string GetConfigurationUrl(int discountId, int? discountRequirementId)
	{
		return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).Action("Configure", "DiscountRulesOrderRange", new { discountId, discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
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
			["Admin.NopStation.DiscountRules.OrderRange.Fields.ConditionValue"] = "Condition",
			["Admin.NopStation.DiscountRules.OrderRange.Fields.ConditionValue.Hint"] = "Selecte a condition.",
			["Admin.NopStation.DiscountRules.OrderRange.Fields.ConditionValue.Required"] = "Condition is required.",
			["Admin.NopStation.DiscountRules.OrderRange.Fields.RangeValue"] = "Range value($)",
			["Admin.NopStation.DiscountRules.OrderRange.Fields.RangeValue.Hint"] = "Give a Range Value.",
			["Admin.NopStation.DiscountRules.OrderRange.Fields.RangeValue.Range"] = "Range Vlaue should be between 1 to 1 Billion.",
			["NopStation.DiscountRules.OrderRange.InvalidForOrderRange"] = "Sorry, this offer is not valid for your selected Order Range."
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
