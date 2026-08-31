using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Helpers;
using NopStation.Plugin.Misc.CustomerReminders.Services;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Controllers;

public class ReminderRuleController : NopStationAdminController
{
	private readonly IReminderRuleModelFactory _reminderRuleModelFactory;

	private readonly IReminderRuleService _reminderRuleService;

	private readonly ILocalizationService _localizationService;

	private readonly INotificationService _notificationService;

	public ReminderRuleController(IReminderRuleModelFactory reminderRuleModelFactory, IReminderRuleService reminderRuleService, ILocalizationService localizationService, INotificationService notificationService)
	{
		_reminderRuleModelFactory = reminderRuleModelFactory;
		_reminderRuleService = reminderRuleService;
		_localizationService = localizationService;
		_notificationService = notificationService;
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> List()
	{
		return View(await _reminderRuleModelFactory.PrepareReminderRuleSearchModelAsync(new ReminderRuleSearchModel()));
	}

	[HttpPost]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> List(ReminderRuleSearchModel searchModel)
	{
		return Json(await _reminderRuleModelFactory.PrepareReminderRuleListModelAsync(searchModel));
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual IActionResult Create()
	{
		ReminderRuleModel model = _reminderRuleModelFactory.PrepareReminderRuleModel(new ReminderRuleModel(), null);
		return View(model);
	}

	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Create(ReminderRuleModel model, bool continueEditing)
	{
		if (base.ModelState.IsValid)
		{
			ReminderRule reminderRule = model.ToEntity<ReminderRule>();
			reminderRule.AvailableTokens = ((model.SelectedTokenList != null) ? ReminderRuleTokenGroupHelper.JoinTokenGroups(model.SelectedTokenList) : string.Empty);
			await _reminderRuleService.InsertReminderRuleAsync(reminderRule);
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Notifications.Added"));
			if (!continueEditing)
			{
				return RedirectToAction("List");
			}
			return RedirectToAction("Edit", new
			{
				id = reminderRule.Id
			});
		}
		model = _reminderRuleModelFactory.PrepareReminderRuleModel(model, null, excludeProperties: true);
		return View(model);
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Edit(int id)
	{
		ReminderRule reminderRule = await _reminderRuleService.GetReminderRuleByIdAsync(id);
		if (reminderRule == null)
		{
			return RedirectToAction("List");
		}
		ReminderRuleModel model = _reminderRuleModelFactory.PrepareReminderRuleModel(null, reminderRule);
		return View(model);
	}

	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Edit(ReminderRuleModel model, bool continueEditing)
	{
		ReminderRule reminderRule = await _reminderRuleService.GetReminderRuleByIdAsync(model.Id);
		if (reminderRule == null)
		{
			return RedirectToAction("List");
		}
		if (base.ModelState.IsValid)
		{
			reminderRule = model.ToEntity(reminderRule);
			reminderRule.AvailableTokens = ((model.SelectedTokenList != null) ? ReminderRuleTokenGroupHelper.JoinTokenGroups(model.SelectedTokenList) : string.Empty);
			await _reminderRuleService.UpdateReminderRuleAsync(reminderRule);
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Notifications.Updated"));
			if (!continueEditing)
			{
				return RedirectToAction("List");
			}
			return RedirectToAction("Edit", new
			{
				id = reminderRule.Id
			});
		}
		model = _reminderRuleModelFactory.PrepareReminderRuleModel(model, reminderRule, excludeProperties: true);
		return View(model);
	}

	[HttpPost]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Delete(int id)
	{
		ReminderRule reminderRule = await _reminderRuleService.GetReminderRuleByIdAsync(id);
		if (reminderRule == null)
		{
			return RedirectToAction("List");
		}
		await _reminderRuleService.DeleteReminderRuleAsync(reminderRule);
		return new NullJsonResult();
	}
}
