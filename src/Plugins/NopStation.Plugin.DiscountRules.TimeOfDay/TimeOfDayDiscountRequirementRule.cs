using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.TimeOfDay;

public class TimeOfDayDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, IPlugin, INopStationPlugin
{
	private readonly IActionContextAccessor _actionContextAccessor;

	private readonly ICustomerService _customerService;

	private readonly IDiscountService _discountService;

	private readonly ILocalizationService _localizationService;

	private readonly ISettingService _settingService;

	private readonly IUrlHelperFactory _urlHelperFactory;

	private readonly IWebHelper _webHelper;

	private readonly IAffiliateService _affiliateService;

	private readonly IPaymentPluginManager _paymentPluginManager;

	private readonly IGenericAttributeService _genericAttributeService;

	public TimeOfDayDiscountRequirementRule(IActionContextAccessor actionContextAccessor, ICustomerService customerService, IDiscountService discountService, ILocalizationService localizationService, ISettingService settingService, IUrlHelperFactory urlHelperFactory, IWebHelper webHelper, IAffiliateService affiliateService, IPaymentPluginManager paymentPluginManager, IGenericAttributeService genericAttributeService)
	{
		_actionContextAccessor = actionContextAccessor;
		_customerService = customerService;
		_discountService = discountService;
		_localizationService = localizationService;
		_settingService = settingService;
		_urlHelperFactory = urlHelperFactory;
		_webHelper = webHelper;
		_affiliateService = affiliateService;
		_paymentPluginManager = paymentPluginManager;
		_genericAttributeService = genericAttributeService;
	}

	public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		DiscountRequirementValidationResult result = new DiscountRequirementValidationResult();
		DateTime timeFrom = await _settingService.GetSettingByKeyAsync<DateTime>($"DiscountRequirement.TimeOfDay-From-{request.DiscountRequirementId}");
		DateTime dateTime = await _settingService.GetSettingByKeyAsync<DateTime>($"DiscountRequirement.TimeOfDay-To-{request.DiscountRequirementId}");
		if (timeFrom == DateTime.MinValue || dateTime == DateTime.MinValue)
		{
			result.IsValid = false;
			return result;
		}
		if (request.Customer == null)
		{
			result.IsValid = false;
			return result;
		}
		if (DateTime.Today.TimeOfDay >= timeFrom.TimeOfDay && DateTime.Today.TimeOfDay <= dateTime.TimeOfDay)
		{
			result.IsValid = true;
		}
		else
		{
			DiscountRequirementValidationResult discountRequirementValidationResult = result;
			discountRequirementValidationResult.UserError = await _localizationService.GetResourceAsync("NopStation.DiscountRules.TimeOfDay.InvalidForTimeOfDay");
		}
		return result;
	}

	public string GetConfigurationUrl(int discountId, int? discountRequirementId)
	{
		return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).Action("Configure", "DiscountRulesTimeOfDay", new { discountId, discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
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
			["Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayFrom"] = "From",
			["Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayFrom.Hint"] = "Discount will be applied from.",
			["Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayTo"] = "To",
			["Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayTo.Hint"] = "Discount will be applied to.",
			["Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayTo.Invalid"] = "Invalid time range",
			["Admin.NopStation.DiscountRules.TimeOfDay.Fields.DiscountId.Required"] = "Discount is required",
			["NopStation.DiscountRules.TimeOfDay.InvalidForTimeOfDay"] = "Sorry, this offer is not valid for this moment."
		};
	}

	public override async Task UninstallAsync()
	{
		IEnumerable<DiscountRequirement> enumerable = (await _discountService.GetAllDiscountRequirementsAsync()).Where((DiscountRequirement discountRequirement) => discountRequirement.DiscountRequirementRuleSystemName == "NopStation.Plugin.DiscountRules.TimeOfDay");
		foreach (DiscountRequirement item in enumerable)
		{
			await _discountService.DeleteDiscountRequirementAsync(item, recursively: false);
		}
		await this.UninstallPluginAsync();
		await base.UninstallAsync();
	}
}
