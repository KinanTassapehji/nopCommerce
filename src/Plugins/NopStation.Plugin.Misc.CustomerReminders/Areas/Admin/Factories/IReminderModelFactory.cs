using System.Threading.Tasks;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;

public interface IReminderModelFactory
{
	Task<ReminderSearchModel> PrepareReminderSearchModelAsync(ReminderSearchModel searchModel);

	Task<ReminderListModel> PrepareReminderListModelAsync(ReminderSearchModel searchModel);

	Task<ReminderModel> PrepareReminderModelAsync(ReminderModel model, Reminder reminder, bool excludeProperties = false);

	Task<ReminderExcludedCustomerSearchModel> PrepareReminderExcludedCustomerSearchModelAsync(ReminderExcludedCustomerSearchModel searchModel, Reminder reminder);

	Task<ReminderExcludedCustomerListModel> PrepareReminderExcludedCustomerListModelAsync(ReminderExcludedCustomerSearchModel searchModel, Reminder reminder);

	Task<AddCustomerToReminderSearchModel> PrepareAddCustomerToReminderSearchModelAsync(AddCustomerToReminderSearchModel searchModel);

	Task<AddCustomerToReminderListModel> PrepareAddCustomerToReminderListModelAsync(AddCustomerToReminderSearchModel searchModel, int reminderId);
}
