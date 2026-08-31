using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core.Domain.Discounts;
using Nop.Services.Discounts;
using Nop.Services.Security;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.DiscountRules.CustomerBirthday.Models;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.DiscountRules.CustomerBirthday.Controllers;

public class DiscountRulesCustomerBirthdayController : NopStationAdminController
{
	private readonly IDiscountService _discountService;

	private readonly IPermissionService _permissionService;

	public DiscountRulesCustomerBirthdayController(IDiscountService discountService, IPermissionService permissionService)
	{
		_discountService = discountService;
		_permissionService = permissionService;
	}

	[CheckPermission("Promotions.DiscountsCreateEditDelete", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
	{
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
		RequirementModel model = new RequirementModel
		{
			RequirementId = discountRequirementId.GetValueOrDefault(),
			DiscountId = discountId
		};
		base.ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HtmlFieldPrefix, discountRequirementId.GetValueOrDefault());
		return View("~/Plugins/NopStation.Plugin.DiscountRules.CustomerBirthday/Views/Configure.cshtml", model);
	}

	[HttpPost]
	[CheckPermission("Promotions.DiscountsCreateEditDelete", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(RequirementModel model)
	{
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
