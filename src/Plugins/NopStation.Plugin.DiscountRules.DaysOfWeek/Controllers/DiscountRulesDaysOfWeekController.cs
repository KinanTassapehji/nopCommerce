using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core.Domain.Discounts;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Security;
using NopStation.Plugin.DiscountRules.DaysOfWeek.Models;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.DiscountRules.DaysOfWeek.Controllers;

public class DiscountRulesDaysOfWeekController : NopStationAdminController
{
	private readonly IDiscountService _discountService;

	private readonly IPermissionService _permissionService;

	private readonly ISettingService _settingService;

	public DiscountRulesDaysOfWeekController(IDiscountService discountService, ISettingService settingService, IPermissionService permissionService)
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
		List<int> daysOfWeek = await _settingService.GetSettingByKeyAsync<List<int>>($"DiscountRequirement.DaysOfWeek-{discountRequirementId.GetValueOrDefault()}");
		RequirementModel requirementModel = new RequirementModel
		{
			RequirementId = discountRequirementId.GetValueOrDefault(),
			DiscountId = discountId,
			DaysOfWeek = daysOfWeek
		};
		RequirementModel requirementModel2 = requirementModel;
		requirementModel2.AvailableDaysOfWeeks = (await CustomDayOfWeek.Friday.ToSelectListAsync()).ToList();
		RequirementModel model = requirementModel;
		base.ViewData.TemplateInfo.HtmlFieldPrefix = $"DiscountRulesDaysOfWeek{discountRequirementId.GetValueOrDefault()}";
		return View("~/Plugins/NopStation.Plugin.DiscountRules.DaysOfWeek/Views/Configure.cshtml", model);
	}

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
					DiscountRequirementRuleSystemName = "NopStation.Plugin.DiscountRules.DaysOfWeek"
				};
				await _discountService.InsertDiscountRequirementAsync(discountRequirement);
			}
			await _settingService.SetSettingAsync($"DiscountRequirement.DaysOfWeek-{discountRequirement.Id}", model.DaysOfWeek.ToList());
			return Ok(new
			{
				NewRequirementId = discountRequirement.Id
			});
		}
		return BadRequest(new
		{
			Errors = GetErrorsFromModelState()
		});
	}

	private IEnumerable<string> GetErrorsFromModelState()
	{
		return base.ModelState.Values.SelectMany((ModelStateEntry v) => v.Errors.Select((ModelError e) => e.ErrorMessage));
	}
}
