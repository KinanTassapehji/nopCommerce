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
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.CustomerGender;

public class CustomerGenderDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, IPlugin, INopStationPlugin
{
	private readonly IActionContextAccessor _actionContextAccessor;

	private readonly IUrlHelperFactory _urlHelperFactory;

	private readonly IWebHelper _webHelper;

	private readonly IDiscountService _discountService;

	private readonly ISettingService _settingService;

	private readonly IWorkContext _workContext;

	public CustomerGenderDiscountRequirementRule(IWorkContext workContext, IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory, IWebHelper webHelper, IDiscountService discountService, ISettingService settingService)
	{
		_workContext = workContext;
		_actionContextAccessor = actionContextAccessor;
		_urlHelperFactory = urlHelperFactory;
		_webHelper = webHelper;
		_discountService = discountService;
		_settingService = settingService;
	}

	public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
	{
		string gender = (await _workContext.GetCurrentCustomerAsync()).Gender;
		DiscountRequirementValidationResult result = new DiscountRequirementValidationResult
		{
			IsValid = false
		};
		if (await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.CustomerGenderSettingsKey, request.DiscountRequirementId)) == gender)
		{
			result.IsValid = true;
		}
		return result;
	}

	public string GetConfigurationUrl(int discountId, int? discountRequirementId)
	{
		return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).Action("Configure", "DiscountRulesCustomerGender", new { discountId, discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
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
			["Admin.NopStation.DiscountRules.CustomerGender.Fields.Gender"] = "Gender",
			["Admin.NopStation.DiscountRules.CustomerGender.Fields.DaysOfWeek.Hint"] = "Discount will be applied on specific gender.",
			["Admin.NopStation.DiscountRules.CustomerGender.Fields.Gender.Required"] = "Select Gender",
			["NopStation.DiscountRules.DaysOfWeek.InvalidForDaysOfWeek"] = "Sorry, this offer is not valid for today."
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
