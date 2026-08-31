using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services;

public interface IReminderRuleService
{
	Task<ReminderRule> GetReminderRuleByIdAsync(int reminderRuleId);

	Task<ReminderRule> GetReminderRuleBySystemNameAsync(string systemName);

	Task<IPagedList<ReminderRule>> GetAllReminderRulesAsync(bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

	Task InsertReminderRuleAsync(ReminderRule reminderRule);

	Task UpdateReminderRuleAsync(ReminderRule reminderRule);

	Task DeleteReminderRuleAsync(ReminderRule reminderRule);

	Task<bool> IsSystemNameUniqueAsync(string systemName, int currentReminderRuleId = 0);
}
