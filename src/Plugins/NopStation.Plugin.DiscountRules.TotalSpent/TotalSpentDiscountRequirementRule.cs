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
using Nop.Services.Orders;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.TotalSpent;

public class TotalSpentDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, IPlugin, INopStationPlugin
{
	private readonly IActionContextAccessor _actionContextAccessor;

	private readonly IDiscountService _discountService;

	private readonly IOrderService _orderService;

	private readonly ISettingService _settingService;

	private readonly IUrlHelperFactory _urlHelperFactory;

	private readonly IWebHelper _webHelper;

	public TotalSpentDiscountRequirementRule(IActionContextAccessor actionContextAccessor, IDiscountService discountService, IOrderService orderService, ISettingService settingService, IUrlHelperFactory urlHelperFactory, IWebHelper webHelper)
	{
		_actionContextAccessor = actionContextAccessor;
		_discountService = discountService;
		_orderService = orderService;
		_settingService = settingService;
		_urlHelperFactory = urlHelperFactory;
		_webHelper = webHelper;
	}

	public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
	{
		DiscountRequirementValidationResult result = new DiscountRequirementValidationResult
		{
			IsValid = false
		};
		if (request.Customer == null || request.Store == null)
		{
			return result;
		}
		decimal amountRequirement = await _settingService.GetSettingByKeyAsync(string.Format(DiscountRequirementDefaults.AmountSettingsKey, request.DiscountRequirementId), 0m);
		if (amountRequirement <= 0m)
		{
			return result;
		}
		IOrderService orderService = _orderService;
		int id = request.Store.Id;
		int id2 = request.Customer.Id;
		List<int> osIds = new List<int> { 30 };
		if ((await orderService.SearchOrdersAsync(id, 0, id2, 0, 0, 0, 0, null, null, null, osIds)).Sum((Order order) => order.OrderTotal) > amountRequirement)
		{
			result.IsValid = true;
		}
		return result;
	}

	public string GetConfigurationUrl(int discountId, int? discountRequirementId)
	{
		return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).Action("Configure", "DiscountRulesTotalSpent", new { discountId, discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
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
			["Admin.NopStation.DiscountRules.TotalSpent.Fields.Amount"] = "Spent amount",
			["Admin.NopStation.DiscountRules.TotalSpent.Fields.Amount.Hint"] = "Enter the total spent amount required for the discount.",
			["Admin.NopStation.DiscountRules.TotalSpent.Fields.Amount.Required"] = "Spent amount is required.",
			["Admin.NopStation.DiscountRules.TotalSpent.Fields.Amount.Range"] = "Spent amount should be between 1 and 1 billion.",
			["NopStation.DiscountRules.TotalSpent.InvalidForTotalSpent"] = "Sorry, this offer is not valid for your total spent amount."
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
