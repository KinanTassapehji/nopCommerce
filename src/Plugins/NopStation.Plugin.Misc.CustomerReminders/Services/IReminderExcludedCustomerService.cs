using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services;

public interface IReminderExcludedCustomerService
{
	Task<ReminderExcludedCustomer> GetReminderExcludedCustomerByIdAsync(int id);

	Task<ReminderExcludedCustomer> GetReminderExcludedCustomerAsync(int reminderId, int customerId);

	Task<IPagedList<ReminderExcludedCustomer>> GetAllReminderExcludedCustomersAsync(int? reminderId = null, int? customerId = null, int pageIndex = 0, int pageSize = int.MaxValue);

	Task InsertReminderExcludedCustomerAsync(ReminderExcludedCustomer reminderExcludedCustomer);

	Task DeleteReminderExcludedCustomerAsync(ReminderExcludedCustomer reminderExcludedCustomer);

	Task<bool> IsCustomerExcludedAsync(int reminderId, int customerId);
}
