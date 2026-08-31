using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

public record ReminderReportModel : BaseNopEntityModel
{
	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.ReminderName")]
	public string ReminderName { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.CustomerName")]
	public string CustomerName { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.CustomerEmail")]
	public string CustomerEmail { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.StoreName")]
	public string StoreName { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.IsMessageSent")]
	public bool IsMessageSent { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.CreatedOn")]
	public DateTime CreatedOnUtc { get; set; }
}
