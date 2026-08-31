using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

public record AddCustomerToReminderModel : BaseNopModel
{
	public int ReminderId { get; set; }

	public int[] SelectedCustomerIds { get; set; }

	public int Id { get; set; }

	public string Email { get; set; }

	public string Username { get; set; }

	public string FullName { get; set; }

	public string CustomerRoleNames { get; set; }

	public bool Active { get; set; }
}
