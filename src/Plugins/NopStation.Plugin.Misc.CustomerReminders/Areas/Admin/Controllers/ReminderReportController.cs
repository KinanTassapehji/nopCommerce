using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Helpers;
using Nop.Services.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Services;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Controllers;

public class ReminderReportController : NopStationAdminController
{
	private readonly IReminderReportExportService _reminderReportExportService;

	private readonly IReminderReportModelFactory _reminderReportModelFactory;

	private readonly IReminderReportService _reminderReportService;

	private readonly INotificationService _notificationService;

	private readonly IDateTimeHelper _dateTimeHelper;

	public ReminderReportController(IReminderReportExportService reminderReportExportService, IReminderReportModelFactory reminderReportModelFactory, IReminderReportService reminderReportService, INotificationService notificationService, IDateTimeHelper dateTimeHelper)
	{
		_reminderReportExportService = reminderReportExportService;
		_reminderReportModelFactory = reminderReportModelFactory;
		_reminderReportService = reminderReportService;
		_notificationService = notificationService;
		_dateTimeHelper = dateTimeHelper;
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> List()
	{
		return View(await _reminderReportModelFactory.PrepareReminderReportSearchModelAsync(new ReminderReportSearchModel()));
	}

	[HttpPost]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> List(ReminderReportSearchModel searchModel)
	{
		return Json(await _reminderReportModelFactory.PrepareReminderReportListModelAsync(searchModel));
	}

	[HttpPost]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
	{
		if (selectedIds == null || !selectedIds.Any())
		{
			return NoContent();
		}
		IList<ReminderReport> reminderReports = await _reminderReportService.GetReminderReportsByIdsAsync(selectedIds.ToArray());
		await _reminderReportService.DeleteReminderReportsAsync(reminderReports);
		return Json(new
		{
			Result = true
		});
	}

	[HttpPost]
	[ActionName("ExportToXml")]
	[FormValueRequired(new string[] { "exportxml-all" })]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ExportXmlAll(ReminderReportSearchModel searchModel)
	{
		int? reminderId = null;
		int? customerId = null;
		int? storeId = ((searchModel.StoreNameId > 0) ? new int?(searchModel.StoreNameId) : ((int?)null));
		bool? isMessageSent = searchModel.IsMessageSentId switch
		{
			1 => false, 
			2 => true, 
			_ => null, 
		};
		DateTime? dateTime;
		if (!searchModel.CreatedFrom.HasValue)
		{
			dateTime = null;
		}
		else
		{
			IDateTimeHelper dateTimeHelper = _dateTimeHelper;
			DateTime value = searchModel.CreatedFrom.Value;
			dateTime = dateTimeHelper.ConvertToUtcTime(value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
		}
		DateTime? createdFromUtc = dateTime;
		if (!searchModel.CreatedTo.HasValue)
		{
			dateTime = null;
		}
		else
		{
			IDateTimeHelper dateTimeHelper = _dateTimeHelper;
			DateTime value = searchModel.CreatedTo.Value;
			dateTime = dateTimeHelper.ConvertToUtcTime(value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1.0);
		}
		DateTime? createdToUtc = dateTime;
		IPagedList<ReminderReport> reports = await _reminderReportService.GetAllReminderReportsAsync(reminderId, searchModel.ReminderName, customerId, searchModel.CustomerName, searchModel.CustomerEmail, storeId, null, isMessageSent, createdFromUtc, createdToUtc);
		IActionResult result = default(IActionResult);
		object obj;
		int num;
		try
		{
			string s = await _reminderReportExportService.ExportReminderReportsToXmlAsync(reports);
			result = File(Encoding.UTF8.GetBytes(s), MimeTypes.ApplicationXml, CustomerRemindersDefaults.Export.ReminderReportsXmlFileName);
			return result;
		}
		catch (Exception ex)
		{
			obj = ex;
			num = 1;
		}
		if (num != 1)
		{
			return result;
		}
		Exception exception = (Exception)obj;
		await _notificationService.ErrorNotificationAsync(exception);
		return RedirectToAction("List");
	}

	[HttpPost]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ExportXmlSelected(string selectedIds)
	{
		List<ReminderReport> reports = new List<ReminderReport>();
		if (!string.IsNullOrEmpty(selectedIds))
		{
			int[] ids = (from x in selectedIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
				select Convert.ToInt32(x)).ToArray();
			List<ReminderReport> list = reports;
			list.AddRange(await _reminderReportService.GetReminderReportsByIdsAsync(ids));
		}
		try
		{
			string s = await _reminderReportExportService.ExportReminderReportsToXmlAsync(reports);
			return File(Encoding.UTF8.GetBytes(s), MimeTypes.ApplicationXml, CustomerRemindersDefaults.Export.ReminderReportsXmlFileName);
		}
		catch (Exception exception)
		{
			await _notificationService.ErrorNotificationAsync(exception);
			return RedirectToAction("List");
		}
	}

	[HttpPost]
	[ActionName("ExportToExcel")]
	[FormValueRequired(new string[] { "exportexcel-all" })]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ExportExcelAll(ReminderReportSearchModel searchModel)
	{
		int? reminderId = null;
		int? customerId = null;
		int? storeId = ((searchModel.StoreNameId > 0) ? new int?(searchModel.StoreNameId) : ((int?)null));
		bool? isMessageSent = searchModel.IsMessageSentId switch
		{
			1 => false, 
			2 => true, 
			_ => null, 
		};
		DateTime? dateTime;
		if (!searchModel.CreatedFrom.HasValue)
		{
			dateTime = null;
		}
		else
		{
			IDateTimeHelper dateTimeHelper = _dateTimeHelper;
			DateTime value = searchModel.CreatedFrom.Value;
			dateTime = dateTimeHelper.ConvertToUtcTime(value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
		}
		DateTime? createdFromUtc = dateTime;
		if (!searchModel.CreatedTo.HasValue)
		{
			dateTime = null;
		}
		else
		{
			IDateTimeHelper dateTimeHelper = _dateTimeHelper;
			DateTime value = searchModel.CreatedTo.Value;
			dateTime = dateTimeHelper.ConvertToUtcTime(value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1.0);
		}
		DateTime? createdToUtc = dateTime;
		IPagedList<ReminderReport> reports = await _reminderReportService.GetAllReminderReportsAsync(reminderId, searchModel.ReminderName, customerId, searchModel.CustomerName, searchModel.CustomerEmail, storeId, null, isMessageSent, createdFromUtc, createdToUtc);
		IActionResult result = default(IActionResult);
		object obj;
		int num;
		try
		{
			result = File(await _reminderReportExportService.ExportReminderReportsToXlsxAsync(reports), MimeTypes.TextXlsx, CustomerRemindersDefaults.Export.ReminderReportsXlsxFileName);
			return result;
		}
		catch (Exception ex)
		{
			obj = ex;
			num = 1;
		}
		if (num != 1)
		{
			return result;
		}
		Exception exception = (Exception)obj;
		await _notificationService.ErrorNotificationAsync(exception);
		return RedirectToAction("List");
	}

	[HttpPost]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ExportExcelSelected(string selectedIds)
	{
		List<ReminderReport> reports = new List<ReminderReport>();
		if (!string.IsNullOrEmpty(selectedIds))
		{
			int[] ids = (from x in selectedIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
				select Convert.ToInt32(x)).ToArray();
			List<ReminderReport> list = reports;
			list.AddRange(await _reminderReportService.GetReminderReportsByIdsAsync(ids));
		}
		try
		{
			return File(await _reminderReportExportService.ExportReminderReportsToXlsxAsync(reports), MimeTypes.TextXlsx, CustomerRemindersDefaults.Export.ReminderReportsXlsxFileName);
		}
		catch (Exception exception)
		{
			await _notificationService.ErrorNotificationAsync(exception);
			return RedirectToAction("List");
		}
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ReminderNameAutoComplete(string term)
	{
		if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
		{
			return Json(new List<object>());
		}
		var data = (await _reminderReportService.GetDistinctReminderNamesAsync(term)).Select((string name) => new
		{
			label = name,
			value = name
		}).ToList();
		return Json(data);
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> CustomerNameAutoComplete(string term)
	{
		if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
		{
			return Json(new List<object>());
		}
		var data = (await _reminderReportService.GetDistinctCustomerNamesAsync(term)).Select((string name) => new
		{
			label = name,
			value = name
		}).ToList();
		return Json(data);
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> CustomerEmailAutoComplete(string term)
	{
		if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
		{
			return Json(new List<object>());
		}
		var data = (await _reminderReportService.GetDistinctCustomerEmailsAsync(term)).Select((string email) => new
		{
			label = email,
			value = email
		}).ToList();
		return Json(data);
	}
}
