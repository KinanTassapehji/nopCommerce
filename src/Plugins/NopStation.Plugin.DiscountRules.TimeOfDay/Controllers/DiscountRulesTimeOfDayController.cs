using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core.Domain.Discounts;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using NopStation.Plugin.DiscountRules.TimeOfDay.Models;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.DiscountRules.TimeOfDay.Controllers;

public class DiscountRulesTimeOfDayController : NopStationAdminController
{
	private readonly IDiscountService _discountService;

	private readonly IPermissionService _permissionService;

	private readonly ILocalizationService _localizationService;

	private readonly ISettingService _settingService;

	public DiscountRulesTimeOfDayController(IDiscountService discountService, ISettingService settingService, IPermissionService permissionService, ILocalizationService localizationService)
	{
		_discountService = discountService;
		_permissionService = permissionService;
		_localizationService = localizationService;
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
		DateTime timeFrom = await _settingService.GetSettingByKeyAsync<DateTime>($"DiscountRequirement.TimeOfDay-From-{discountRequirementId.GetValueOrDefault()}");
		DateTime timeOfDayTo = await _settingService.GetSettingByKeyAsync<DateTime>($"DiscountRequirement.TimeOfDay-To-{discountRequirementId.GetValueOrDefault()}");
		RequirementModel model = new RequirementModel
		{
			RequirementId = discountRequirementId.GetValueOrDefault(),
			DiscountId = discountId,
			TimeOfDayFrom = timeFrom,
			TimeOfDayTo = timeOfDayTo
		};
		base.ViewData.TemplateInfo.HtmlFieldPrefix = $"DiscountRulesTimeOfDay{discountRequirementId.GetValueOrDefault()}";
		return View("~/Plugins/NopStation.Plugin.DiscountRules.TimeOfDay/Views/Configure.cshtml", model);
	}

	[HttpPost]
	public async Task<IActionResult> Configure(RequirementModel model)
	{
		if (!(await _permissionService.AuthorizeAsync("Promotions.DiscountsCreateEditDelete")))
		{
			return Content("Access denied");
		}
		if (model.TimeOfDayFrom.TimeOfDay >= model.TimeOfDayTo.TimeOfDay)
		{
			ModelStateDictionary modelState = base.ModelState;
			modelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayTo.Invalid"));
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
					DiscountRequirementRuleSystemName = "NopStation.Plugin.DiscountRules.TimeOfDay"
				};
				await _discountService.InsertDiscountRequirementAsync(discountRequirement);
			}
			await _settingService.SetSettingAsync($"DiscountRequirement.TimeOfDay-From-{discountRequirement.Id}", model.TimeOfDayFrom);
			await _settingService.SetSettingAsync($"DiscountRequirement.TimeOfDay-To-{discountRequirement.Id}", model.TimeOfDayTo);
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
