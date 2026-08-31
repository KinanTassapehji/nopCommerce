using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

public record ReminderExcludedCustomerModel : BaseNopEntityModel
{
	public int ReminderId { get; set; }

	public int CustomerId { get; set; }

	public string CustomerEmail { get; set; }

	public string CustomerName { get; set; }
}
