using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.DaysOfWeek;

public class DaysOfWeekDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, IPlugin, INopStationPlugin
{
	private readonly IActionContextAccessor _actionContextAccessor;

	private readonly IDiscountService _discountService;

	private readonly ILocalizationService _localizationService;

	private readonly ISettingService _settingService;

	private readonly IUrlHelperFactory _urlHelperFactory;

	private readonly IWebHelper _webHelper;

	public DaysOfWeekDiscountRequirementRule(IActionContextAccessor actionContextAccessor, IDiscountService discountService, ILocalizationService localizationService, ISettingService settingService, IUrlHelperFactory urlHelperFactory, IWebHelper webHelper)
	{
		_actionContextAccessor = actionContextAccessor;
		_discountService = discountService;
		_localizationService = localizationService;
		_settingService = settingService;
		_urlHelperFactory = urlHelperFactory;
		_webHelper = webHelper;
	}

	public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		DiscountRequirementValidationResult result = new DiscountRequirementValidationResult();
		List<int> list = await _settingService.GetSettingByKeyAsync<List<int>>($"DiscountRequirement.DaysOfWeek-{request.DiscountRequirementId}");
		if (list == null || !list.Any())
		{
			result.IsValid = false;
			return result;
		}
		if (request.Customer == null)
		{
			result.IsValid = false;
			return result;
		}
		if (list.Contains((int)(DateTime.UtcNow.DayOfWeek + 1)))
		{
			result.IsValid = true;
		}
		else
		{
			DiscountRequirementValidationResult discountRequirementValidationResult = result;
			discountRequirementValidationResult.UserError = await _localizationService.GetResourceAsync("NopStation.DiscountRules.DaysOfWeek.InvalidForDaysOfWeek");
		}
		return result;
	}

	public string GetConfigurationUrl(int discountId, int? discountRequirementId)
	{
		return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).Action("Configure", "DiscountRulesDaysOfWeek", new { discountId, discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
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
			["Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DaysOfWeek"] = "Days of week",
			["Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DaysOfWeek.Hint"] = "Discount will be applied on specific days of week.",
			["Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DaysOfWeek.Required"] = "Select any days of week",
			["Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DiscountId.Required"] = "Discount is required",
			["NopStation.DiscountRules.DaysOfWeek.InvalidForDaysOfWeek"] = "Sorry, this offer is not valid for today."
		};
	}

	public override async Task UninstallAsync()
	{
		IEnumerable<DiscountRequirement> enumerable = (await _discountService.GetAllDiscountRequirementsAsync()).Where((DiscountRequirement discountRequirement) => discountRequirement.DiscountRequirementRuleSystemName == "NopStation.Plugin.DiscountRules.DaysOfWeek");
		foreach (DiscountRequirement item in enumerable)
		{
			await _discountService.DeleteDiscountRequirementAsync(item, recursively: false);
		}
		await this.UninstallPluginAsync();
		await base.UninstallAsync();
	}
}
