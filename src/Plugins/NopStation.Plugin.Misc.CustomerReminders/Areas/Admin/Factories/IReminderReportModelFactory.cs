using System.Threading.Tasks;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;

public interface IReminderReportModelFactory
{
	Task<ReminderReportSearchModel> PrepareReminderReportSearchModelAsync(ReminderReportSearchModel searchModel);

	Task<ReminderReportListModel> PrepareReminderReportListModelAsync(ReminderReportSearchModel searchModel);
}
