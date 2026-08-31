using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Discounts;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Security;
using NopStation.Plugin.DiscountRules.OrderRange.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.DiscountRules.OrderRange.Controllers;

public class DiscountRulesOrderRangeController : NopStationAdminController
{
	private readonly IDiscountService _discountService;

	private readonly IPermissionService _permissionService;

	private readonly ISettingService _settingService;

	public DiscountRulesOrderRangeController(IDiscountService discountService, IPermissionService permissionService, ISettingService settingService)
	{
		_discountService = discountService;
		_permissionService = permissionService;
		_settingService = settingService;
	}

	public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
	{
		if (!(await _permissionService.AuthorizeAsync("Promotions.DiscountsCreateEditDelete")))
		{
			return Content("Access denied");
		}
		if (await _discountService.GetDiscountByIdAsync(discountId) == null)
		{
			throw new ArgumentException("Discount could not be loaded");
		}
		bool flag = discountRequirementId.HasValue;
		if (flag)
		{
			flag = await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value) == null;
		}
		if (flag)
		{
			return Content("Failed to load requirement.");
		}
		string conditionValue = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.ConditionValueSettingsKey, discountRequirementId.GetValueOrDefault()));
		int rangeValue = await _settingService.GetSettingByKeyAsync(string.Format(DiscountRequirementDefaults.RangeValueSettingsKey, discountRequirementId.GetValueOrDefault()), 0);
		RequirementModel requirementModel = new RequirementModel
		{
			RequirementId = discountRequirementId.GetValueOrDefault(),
			DiscountId = discountId,
			ConditionValue = conditionValue,
			RangeValue = rangeValue
		};
		EnumForConditionSelection.Conditons[] array = (EnumForConditionSelection.Conditons[])Enum.GetValues(typeof(EnumForConditionSelection.Conditons));
		for (int i = 0; i < array.Length; i++)
		{
			EnumForConditionSelection.Conditons conditons = array[i];
			char value = (char)conditons;
			requirementModel.AvailableConditions.Add(new SelectListItem
			{
				Text = (conditons.ToString() ?? ""),
				Value = $"{value}",
				Selected = (conditionValue == $"{value}")
			});
		}
		requirementModel.AvailableConditions.Insert(0, new SelectListItem
		{
			Text = "Please selecte a condition",
			Value = "0"
		});
		base.ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HtmlFieldPrefix, discountRequirementId.GetValueOrDefault());
		return View("~/Plugins/NopStation.Plugin.DiscountRules.OrderRange/Views/Configure.cshtml", requirementModel);
	}

	[EditAccess(false)]
	[HttpPost]
	public async Task<IActionResult> Configure(RequirementModel model)
	{
		if (!(await _permissionService.AuthorizeAsync("Promotions.DiscountsCreateEditDelete")))
		{
			return Content("Access denied");
		}
		if (base.ModelState.IsValid)
		{
			Discount discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
			if (discount == null)
			{
				return NotFound(new
				{
					Errors = new string[1] { "Discount could not be loaded" }
				});
			}
			DiscountRequirement discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId);
			if (discountRequirement == null)
			{
				discountRequirement = new DiscountRequirement
				{
					DiscountId = discount.Id,
					DiscountRequirementRuleSystemName = DiscountRequirementDefaults.SystemName
				};
				await _discountService.InsertDiscountRequirementAsync(discountRequirement);
			}
			await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.ConditionValueSettingsKey, discountRequirement.Id), model.ConditionValue);
			await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.RangeValueSettingsKey, discountRequirement.Id), model.RangeValue);
			return Ok(new
			{
				NewRequirementId = discountRequirement.Id
			});
		}
		return Ok(new
		{
			Errors = GetErrorsFromModelState(base.ModelState)
		});
	}

	private IEnumerable<string> GetErrorsFromModelState(ModelStateDictionary modelState)
	{
		return base.ModelState.Values.SelectMany((ModelStateEntry v) => v.Errors.Select((ModelError e) => e.ErrorMessage));
	}
}
