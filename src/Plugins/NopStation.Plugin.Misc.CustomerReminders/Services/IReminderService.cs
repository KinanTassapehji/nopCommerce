using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services;

public interface IReminderService
{
	Task<Reminder> GetReminderByIdAsync(int reminderId);

	Task<IPagedList<Reminder>> GetAllRemindersAsync(string name = null, int storeId = 0, bool? isEnabled = null, int pageIndex = 0, int pageSize = int.MaxValue);

	Task InsertReminderAsync(Reminder reminder);

	Task UpdateReminderAsync(Reminder reminder);

	Task DeleteReminderAsync(Reminder reminder);

	Task<bool> IsNameUniqueAsync(string name, int currentReminderId = 0);

	Task<IList<string>> GetDistinctReminderNamesAsync(string searchTerm = null);
}
