using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

public record ReminderReportSearchModel : BaseSearchModel
{
	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.ReminderName")]
	public string ReminderName { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CustomerName")]
	public string CustomerName { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CustomerEmail")]
	public string CustomerEmail { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.StoreName")]
	public int StoreNameId { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.IsMessageSent")]
	public int IsMessageSentId { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CreatedFrom")]
	[UIHint("DateNullable")]
	public DateTime? CreatedFrom { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CreatedTo")]
	[UIHint("DateNullable")]
	public DateTime? CreatedTo { get; set; }

	public IList<SelectListItem> AvailableStores { get; set; }

	public IList<SelectListItem> AvailableMessageSentOptions { get; set; }

	public ReminderReportSearchModel()
	{
		AvailableStores = new List<SelectListItem>();
		AvailableMessageSentOptions = new List<SelectListItem>();
	}
}
