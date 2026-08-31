using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

public record AddCustomerToReminderSearchModel : BaseSearchModel
{
	public int ReminderId { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.ExcludedCustomers.Search.Email")]
	public string SearchEmail { get; set; }
}
