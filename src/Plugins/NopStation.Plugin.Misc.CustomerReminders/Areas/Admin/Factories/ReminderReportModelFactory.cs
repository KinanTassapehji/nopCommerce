using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Services.Helpers;
using Nop.Services.Stores;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Services;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;

public class ReminderReportModelFactory : IReminderReportModelFactory
{
	private readonly IReminderReportService _reminderReportService;

	private readonly IStoreService _storeService;

	private readonly IDateTimeHelper _dateTimeHelper;

	public ReminderReportModelFactory(IReminderReportService reminderReportService, IStoreService storeService, IDateTimeHelper dateTimeHelper)
	{
		_reminderReportService = reminderReportService;
		_storeService = storeService;
		_dateTimeHelper = dateTimeHelper;
	}

	public async Task<ReminderReportSearchModel> PrepareReminderReportSearchModelAsync(ReminderReportSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		foreach (Store item in await _storeService.GetAllStoresAsync())
		{
			searchModel.AvailableStores.Add(new SelectListItem
			{
				Text = item.Name,
				Value = item.Id.ToString()
			});
		}
		searchModel.AvailableStores.Insert(0, new SelectListItem
		{
			Text = "All",
			Value = "0"
		});
		searchModel.AvailableMessageSentOptions.Add(new SelectListItem
		{
			Text = "All",
			Value = "0"
		});
		searchModel.AvailableMessageSentOptions.Add(new SelectListItem
		{
			Text = "Yes",
			Value = "1"
		});
		searchModel.AvailableMessageSentOptions.Add(new SelectListItem
		{
			Text = "No",
			Value = "2"
		});
		searchModel.SetGridPageSize();
		return searchModel;
	}

	public async Task<ReminderReportListModel> PrepareReminderReportListModelAsync(ReminderReportSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		bool? isMessageSent = ((searchModel.IsMessageSentId == 0) ? ((bool?)null) : new bool?(searchModel.IsMessageSentId == 1));
		string nStoreName = (await _storeService.GetStoreByIdAsync(searchModel.StoreNameId))?.Name;
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
		DateTime? dateTime2 = dateTime;
		IReminderReportService reminderReportService = _reminderReportService;
		string reminderName = searchModel.ReminderName;
		string customerName = searchModel.CustomerName;
		string customerEmail = searchModel.CustomerEmail;
		string storeName = nStoreName;
		bool? isMessageSent2 = isMessageSent;
		dateTime = createdFromUtc;
		DateTime? createdToUtc = dateTime2;
		int pageIndex = searchModel.Page - 1;
		int pageSize = searchModel.PageSize;
		IPagedList<ReminderReport> reports = await reminderReportService.GetAllReminderReportsAsync(null, reminderName, null, customerName, customerEmail, null, storeName, isMessageSent2, dateTime, createdToUtc, pageIndex, pageSize);
		return new ReminderReportListModel().PrepareToGrid(searchModel, reports, () => reports.Select((ReminderReport report) => new ReminderReportModel
		{
			Id = report.Id,
			ReminderName = report.ReminderName,
			CustomerName = report.CustomerName,
			CustomerEmail = report.CustomerEmail,
			StoreName = report.StoreName,
			IsMessageSent = report.IsMessageSent,
			CreatedOnUtc = report.CreatedOnUtc
		}));
	}
}
