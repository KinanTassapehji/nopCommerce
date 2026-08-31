using Nop.Core;

namespace NopStation.Plugin.Misc.CustomerReminders.Domains;

public class ReminderExcludedCustomer : BaseEntity
{
	public int ReminderId { get; set; }

	public int CustomerId { get; set; }
}
