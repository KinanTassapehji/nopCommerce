using System.Threading.Tasks;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;

public interface IReminderRuleModelFactory
{
	Task<ReminderRuleSearchModel> PrepareReminderRuleSearchModelAsync(ReminderRuleSearchModel searchModel);

	Task<ReminderRuleListModel> PrepareReminderRuleListModelAsync(ReminderRuleSearchModel searchModel);

	ReminderRuleModel PrepareReminderRuleModel(ReminderRuleModel model, ReminderRule reminderRule, bool excludeProperties = false);
}
