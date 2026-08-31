using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

public record ReminderSearchModel : BaseSearchModel
{
	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.List.SearchReminderName")]
	public string SearchReminderName { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.List.SearchEnabled")]
	public int SearchEnabledId { get; set; }

	public IList<SelectListItem> AvailableEnabledOptions { get; set; } = new List<SelectListItem>();
}
