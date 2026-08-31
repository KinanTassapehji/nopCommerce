using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Services.Discounts;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.CustomerBirthday;

public class CustomerBirthdayDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, IPlugin, INopStationPlugin
{
	private readonly IActionContextAccessor _actionContextAccessor;

	private readonly IUrlHelperFactory _urlHelperFactory;

	private readonly IWorkContext _workContext;

	private readonly IWebHelper _webHelper;

	private readonly IDiscountService _discountService;

	public CustomerBirthdayDiscountRequirementRule(IWorkContext workContext, IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory, IWebHelper webHelper, IDiscountService discountService)
	{
		_workContext = workContext;
		_actionContextAccessor = actionContextAccessor;
		_urlHelperFactory = urlHelperFactory;
		_webHelper = webHelper;
		_discountService = discountService;
	}

	public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
	{
		DiscountRequirementValidationResult result = new DiscountRequirementValidationResult
		{
			IsValid = false
		};
		DateTime? dateOfBirth = (await _workContext.GetCurrentCustomerAsync()).DateOfBirth;
		if (!dateOfBirth.HasValue)
		{
			return result;
		}
		DateTime now = DateTime.Now;
		if (now.Day == dateOfBirth.Value.Day && now.Month == dateOfBirth.Value.Month)
		{
			result.IsValid = true;
		}
		return result;
	}

	public string GetConfigurationUrl(int discountId, int? discountRequirementId)
	{
		return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).Action("Configure", "DiscountRulesCustomerBirthday", new { discountId, discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
	}

	public override async Task InstallAsync()
	{
		await this.InstallPluginAsync();
		await base.InstallAsync();
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>();
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
