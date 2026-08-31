using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Areas.Admin.Factories;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Domains.SMS;
using NopStation.Plugin.Misc.Core.Services.SMS;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Controllers;

public class QueuedSmsController : NopStationAdminController
{
	private readonly IQueuedSmsService _queuedSmsService;

	private readonly IQueuedSmsModelFactory _queuedSmsModelFactory;

	private readonly ISmsService _smsService;

	private readonly ILocalizationService _localizationService;

	private readonly INotificationService _notificationService;

	private readonly ICustomerService _customerService;

	public QueuedSmsController(IQueuedSmsService queuedSmsService, IQueuedSmsModelFactory queuedSmsModelFactory, ISmsService smsService, ILocalizationService localizationService, INotificationService notificationService, ICustomerService customerService)
	{
		_queuedSmsService = queuedSmsService;
		_queuedSmsModelFactory = queuedSmsModelFactory;
		_smsService = smsService;
		_localizationService = localizationService;
		_notificationService = notificationService;
		_customerService = customerService;
	}

	[CheckPermission("ManageNopStationSmsQueue", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual Task<IActionResult> Index()
	{
		return Task.FromResult((IActionResult)RedirectToAction("List"));
	}

	[CheckPermission("ManageNopStationSmsQueue", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual Task<IActionResult> List()
	{
		QueuedSmsSearchModel model = _queuedSmsModelFactory.PrepareQueuedSmsSearchModel(new QueuedSmsSearchModel());
		return Task.FromResult((IActionResult)View(model));
	}

	[CheckPermission("ManageNopStationSmsQueue", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	[HttpPost]
	public virtual async Task<IActionResult> List(QueuedSmsSearchModel searchModel)
	{
		return Json(await _queuedSmsModelFactory.PrepareQueuedSmsListModelAsync(searchModel));
	}

	[CheckPermission("ManageNopStationSmsQueue", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> View(int id)
	{
		QueuedSms queuedSms = await _queuedSmsService.GetQueuedSmsByIdAsync(id);
		if (queuedSms == null)
		{
			return RedirectToAction("List");
		}
		return View(await _queuedSmsModelFactory.PrepareQueuedSmsModelAsync(null, queuedSms));
	}

	[CheckPermission("ManageNopStationSmsQueue", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	[HttpPost]
	public virtual async Task<IActionResult> Delete(int id)
	{
		QueuedSms queuedSms = await _queuedSmsService.GetQueuedSmsByIdAsync(id);
		if (queuedSms == null)
		{
			return RedirectToAction("List");
		}
		await _queuedSmsService.DeleteQueuedSmsAsync(queuedSms);
		INotificationService notificationService = _notificationService;
		notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.QueuedSms.Deleted"));
		return RedirectToAction("List");
	}

	[CheckPermission("ManageNopStationSmsQueue", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	[HttpPost]
	public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
	{
		if (selectedIds == null || !selectedIds.Any())
		{
			return NoContent();
		}
		foreach (int selectedId in selectedIds)
		{
			QueuedSms queuedSms = await _queuedSmsService.GetQueuedSmsByIdAsync(selectedId);
			if (queuedSms != null)
			{
				await _queuedSmsService.DeleteQueuedSmsAsync(queuedSms);
			}
		}
		return Json(new
		{
			Result = true
		});
	}

	[HttpPost]
	[ActionName("List")]
	[FormValueRequired(new string[] { "delete-all" })]
	[CheckPermission("ManageNopStationSmsQueue", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> DeleteAll()
	{
		await _queuedSmsService.DeleteAllAsync();
		INotificationService notificationService = _notificationService;
		notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.QueuedSms.DeletedAll"));
		return RedirectToAction("List");
	}

	[CheckPermission("ManageNopStationSmsQueue", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	[HttpPost]
	public virtual async Task<IActionResult> RequeueSelected(int[] selectedIds)
	{
		if (selectedIds == null || selectedIds.Length == 0)
		{
			return NoContent();
		}
		IList<QueuedSms> list = await _queuedSmsService.GetQueuedSmsByIdsAsync(selectedIds);
		if (!list.Any())
		{
			return NotFound();
		}
		foreach (QueuedSms item in list)
		{
			QueuedSms queuedSms = new QueuedSms
			{
				PhoneNumber = item.PhoneNumber,
				Body = item.Body,
				CustomerId = item.CustomerId,
				StoreId = item.StoreId,
				ProviderSystemName = item.ProviderSystemName,
				CreatedOnUtc = DateTime.UtcNow
			};
			await _queuedSmsService.InsertQueuedSmsAsync(queuedSms);
		}
		return Json(new
		{
			Result = true
		});
	}

	[CheckPermission("ManageNopStationSmsQueue", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	[HttpPost]
	public virtual async Task<IActionResult> Resend(int id)
	{
		QueuedSms queuedSms = await _queuedSmsService.GetQueuedSmsByIdAsync(id);
		if (queuedSms == null)
		{
			return RedirectToAction("List");
		}
		try
		{
			SmsSendResult smsSendResult = ((!string.IsNullOrWhiteSpace(queuedSms.ProviderSystemName)) ? (await _smsService.SendSmsAsync(queuedSms.PhoneNumber, queuedSms.Body, queuedSms.ProviderSystemName, null, queuedSms.StoreId)) : (await _smsService.SendSmsAsync(queuedSms.PhoneNumber, queuedSms.Body, null, queuedSms.StoreId)));
			SmsSendResult result = smsSendResult;
			if (result.Success)
			{
				queuedSms.SentOnUtc = DateTime.UtcNow;
				queuedSms.ExternalMessageId = result.ExternalMessageId;
				queuedSms.SentTries++;
				await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
				INotificationService notificationService = _notificationService;
				notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.QueuedSms.Resent"));
			}
			else
			{
				queuedSms.SentTries++;
				queuedSms.Error = $"{queuedSms.Error}{queuedSms.SentTries}. {result.Message}<br>";
				await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
				INotificationService notificationService = _notificationService;
				notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.QueuedSms.ResendFailed") + ": " + result.Message);
			}
		}
		catch (Exception ex)
		{
			queuedSms.SentTries++;
			queuedSms.Error = $"{queuedSms.Error}{queuedSms.SentTries}. {ex.Message}<br>";
			await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
			INotificationService notificationService = _notificationService;
			notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.QueuedSms.ResendFailed") + ": " + ex.Message);
		}
		return RedirectToAction("View", new { id });
	}
}
