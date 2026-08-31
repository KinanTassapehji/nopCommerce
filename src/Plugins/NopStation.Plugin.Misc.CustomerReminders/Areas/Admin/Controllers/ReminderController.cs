using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Helpers;
using NopStation.Plugin.Misc.CustomerReminders.Services;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Controllers;

public class ReminderController : NopStationAdminController
{
	private readonly IReminderModelFactory _reminderModelFactory;

	private readonly IReminderService _reminderService;

	private readonly IReminderRuleService _reminderRuleService;

	private readonly IReminderExcludedCustomerService _reminderExcludedCustomerService;

	private readonly IMessageTokenProvider _messageTokenProvider;

	private readonly IMessageTemplateService _messageTemplateService;

	private readonly INotificationService _notificationService;

	private readonly ILocalizationService _localizationService;

	private readonly ILogger _logger;

	public ReminderController(IReminderModelFactory reminderModelFactory, IReminderService reminderService, IReminderRuleService reminderRuleService, IReminderExcludedCustomerService reminderExcludedCustomerService, IMessageTokenProvider messageTokenProvider, IMessageTemplateService messageTemplateService, INotificationService notificationService, ILocalizationService localizationService, ILogger logger)
	{
		_reminderModelFactory = reminderModelFactory;
		_reminderService = reminderService;
		_reminderRuleService = reminderRuleService;
		_reminderExcludedCustomerService = reminderExcludedCustomerService;
		_messageTokenProvider = messageTokenProvider;
		_messageTemplateService = messageTemplateService;
		_notificationService = notificationService;
		_localizationService = localizationService;
		_logger = logger;
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> List()
	{
		return View(await _reminderModelFactory.PrepareReminderSearchModelAsync(new ReminderSearchModel()));
	}

	[HttpPost]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> List(ReminderSearchModel searchModel)
	{
		return Json(await _reminderModelFactory.PrepareReminderListModelAsync(searchModel));
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Create()
	{
		ReminderModel reminderModel = await _reminderModelFactory.PrepareReminderModelAsync(new ReminderModel(), null);
		reminderModel.IsEnabled = true;
		return View(reminderModel);
	}

	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Create(ReminderModel model, bool continueEditing)
	{
		if (base.ModelState.IsValid)
		{
			if (!(await _reminderService.IsNameUniqueAsync(model.Name)))
			{
				ModelStateDictionary modelState = base.ModelState;
				modelState.AddModelError("Name", await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Name.Unique"));
				model = await _reminderModelFactory.PrepareReminderModelAsync(model, null, excludeProperties: true);
				return View(model);
			}
			try
			{
				Reminder reminder = model.ToEntity<Reminder>();
				MessageTemplate messageTemplate = new MessageTemplate
				{
					Name = model.MessageTemplateName,
					BccEmailAddresses = model.MessageTemplateBcc,
					Subject = model.MessageTemplateSubject,
					Body = model.MessageTemplateBody,
					IsActive = true,
					EmailAccountId = model.EmailAccountId,
					DelayBeforeSend = null,
					AttachedDownloadId = 0,
					LimitedToStores = false
				};
				await _messageTemplateService.InsertMessageTemplateAsync(messageTemplate);
				reminder.MessageTemplateId = messageTemplate.Id;
				await _reminderService.InsertReminderAsync(reminder);
				INotificationService notificationService = _notificationService;
				notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Notifications.Added"));
				if (!continueEditing)
				{
					return RedirectToAction("List");
				}
				return RedirectToAction("Edit", new
				{
					id = reminder.Id
				});
			}
			catch (Exception ex)
			{
				await _logger.ErrorAsync("Error creating reminder " + ex.Message, ex);
				ModelStateDictionary modelState = base.ModelState;
				string errorMessage = await _localizationService.GetResourceAsync("Admin.Common.Alert.Save.Error");
				modelState.AddModelError(string.Empty, errorMessage);
			}
		}
		model = await _reminderModelFactory.PrepareReminderModelAsync(model, null, excludeProperties: true);
		return View(model);
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Edit(int id)
	{
		Reminder reminder = await _reminderService.GetReminderByIdAsync(id);
		if (reminder == null)
		{
			return RedirectToAction("List");
		}
		return View(await _reminderModelFactory.PrepareReminderModelAsync(null, reminder));
	}

	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Edit(ReminderModel model, bool continueEditing)
	{
		Reminder reminder = await _reminderService.GetReminderByIdAsync(model.Id);
		if (reminder == null)
		{
			return RedirectToAction("List");
		}
		if (base.ModelState.IsValid)
		{
			if (!(await _reminderService.IsNameUniqueAsync(model.Name, model.Id)))
			{
				ModelStateDictionary modelState = base.ModelState;
				modelState.AddModelError("Name", await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Name.Unique"));
				model = await _reminderModelFactory.PrepareReminderModelAsync(model, reminder, excludeProperties: true);
				return View(model);
			}
			try
			{
				reminder = model.ToEntity(reminder);
				MessageTemplate messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(reminder.MessageTemplateId);
				if (messageTemplate != null)
				{
					messageTemplate.Name = model.MessageTemplateName;
					messageTemplate.BccEmailAddresses = model.MessageTemplateBcc;
					messageTemplate.Subject = model.MessageTemplateSubject;
					messageTemplate.Body = model.MessageTemplateBody;
					messageTemplate.IsActive = true;
					messageTemplate.EmailAccountId = model.EmailAccountId;
					messageTemplate.DelayBeforeSend = null;
					messageTemplate.AttachedDownloadId = 0;
					messageTemplate.LimitedToStores = false;
					await _messageTemplateService.UpdateMessageTemplateAsync(messageTemplate);
				}
				await _reminderService.UpdateReminderAsync(reminder);
				INotificationService notificationService = _notificationService;
				notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Notifications.Updated"));
				if (!continueEditing)
				{
					return RedirectToAction("List");
				}
				return RedirectToAction("Edit", new
				{
					id = reminder.Id
				});
			}
			catch (Exception ex)
			{
				await _logger.ErrorAsync("Error updating reminder " + ex.Message, ex);
				ModelStateDictionary modelState = base.ModelState;
				string errorMessage = await _localizationService.GetResourceAsync("Admin.Common.Alert.Save.Error");
				modelState.AddModelError(string.Empty, errorMessage);
			}
		}
		model = await _reminderModelFactory.PrepareReminderModelAsync(model, reminder, excludeProperties: true);
		return View(model);
	}

	[HttpPost]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Delete(int id)
	{
		Reminder reminder = await _reminderService.GetReminderByIdAsync(id);
		if (reminder == null)
		{
			return RedirectToAction("List");
		}
		if (reminder.MessageTemplateId > 0)
		{
			MessageTemplate messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(reminder.MessageTemplateId);
			if (messageTemplate != null)
			{
				await _messageTemplateService.DeleteMessageTemplateAsync(messageTemplate);
			}
		}
		await _reminderService.DeleteReminderAsync(reminder);
		return new NullJsonResult();
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> GetReminderRuleTokens(int reminderRuleId)
	{
		if (reminderRuleId <= 0)
		{
			return Json(new
			{
				success = false,
				message = "Invalid reminder rule"
			});
		}
		try
		{
			ReminderRule reminderRule = await _reminderRuleService.GetReminderRuleByIdAsync(reminderRuleId);
			if (reminderRule == null || string.IsNullOrEmpty(reminderRule.AvailableTokens))
			{
				return Json(new
				{
					success = true,
					tokens = string.Empty
				});
			}
			List<string> tokenGroups = ReminderRuleTokenGroupHelper.ParseTokenGroups(reminderRule.AvailableTokens);
			string tokens = string.Join(", ", await _messageTokenProvider.GetListOfAllowedTokensAsync(tokenGroups));
			return Json(new
			{
				success = true,
				tokens = tokens
			});
		}
		catch (Exception ex)
		{
			return Json(new
			{
				success = false,
				message = ex.Message
			});
		}
	}

	[HttpPost]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ExcludedCustomerList(ReminderExcludedCustomerSearchModel searchModel)
	{
		Reminder reminder = (await _reminderService.GetReminderByIdAsync(searchModel.ReminderId)) ?? throw new ArgumentException("No reminder found with the specified id", "ReminderId");
		return Json(await _reminderModelFactory.PrepareReminderExcludedCustomerListModelAsync(searchModel, reminder));
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ExcludedCustomerDelete(int id)
	{
		ReminderExcludedCustomer reminderExcludedCustomer = (await _reminderExcludedCustomerService.GetReminderExcludedCustomerByIdAsync(id)) ?? throw new ArgumentException("No reminder excluded customer found with the specified id", "id");
		await _reminderExcludedCustomerService.DeleteReminderExcludedCustomerAsync(reminderExcludedCustomer);
		return new NullJsonResult();
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> CustomerAddPopup(int reminderId)
	{
		return View(await _reminderModelFactory.PrepareAddCustomerToReminderSearchModelAsync(new AddCustomerToReminderSearchModel
		{
			ReminderId = reminderId
		}));
	}

	[HttpPost]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> CustomerAddPopupList(AddCustomerToReminderSearchModel searchModel)
	{
		return Json(await _reminderModelFactory.PrepareAddCustomerToReminderListModelAsync(searchModel, searchModel.ReminderId));
	}

	[HttpPost]
	[FormValueRequired(new string[] { "save" })]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> CustomerAddPopup(AddCustomerToReminderModel model)
	{
		Reminder reminder = (await _reminderService.GetReminderByIdAsync(model.ReminderId)) ?? throw new ArgumentException("No reminder found with the specified id", "ReminderId");
		if (model.SelectedCustomerIds != null && model.SelectedCustomerIds.Length != 0)
		{
			int[] selectedCustomerIds = model.SelectedCustomerIds;
			foreach (int customerId in selectedCustomerIds)
			{
				if (await _reminderExcludedCustomerService.GetReminderExcludedCustomerAsync(reminder.Id, customerId) == null)
				{
					await _reminderExcludedCustomerService.InsertReminderExcludedCustomerAsync(new ReminderExcludedCustomer
					{
						ReminderId = reminder.Id,
						CustomerId = customerId
					});
				}
			}
		}
		base.ViewBag.RefreshPage = true;
		return View(new AddCustomerToReminderSearchModel
		{
			ReminderId = model.ReminderId
		});
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ReminderNameAutoComplete(string term)
	{
		if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
		{
			return Json(new List<object>());
		}
		var data = (await _reminderService.GetDistinctReminderNamesAsync(term)).Select((string name) => new
		{
			label = name,
			value = name
		}).ToList();
		return Json(data);
	}
}
