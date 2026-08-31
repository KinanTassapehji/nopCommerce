using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services;

public interface IReminderReportService
{
	Task<ReminderReport> GetReminderReportByIdAsync(int id);

	Task<IPagedList<ReminderReport>> GetAllReminderReportsAsync(int? reminderId = null, string reminderName = null, int? customerId = null, string customerName = null, string customerEmail = null, int? storeId = null, string storeName = null, bool? isMessageSent = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue);

	Task<int> GetSentMessageCountAsync(int customerId, int reminderId);

	Task<ReminderReport> GetLastSentReportAsync(int customerId, int reminderId);

	Task InsertReminderReportAsync(ReminderReport reminderReport);

	Task DeleteReminderReportAsync(ReminderReport reminderReport);

	Task<IList<ReminderReport>> GetReminderReportsByIdsAsync(int[] ids);

	Task DeleteReminderReportsAsync(IList<ReminderReport> reminderReports);

	Task<IList<string>> GetDistinctReminderNamesAsync(string searchTerm = null);

	Task<IList<string>> GetDistinctCustomerNamesAsync(string searchTerm = null);

	Task<IList<string>> GetDistinctCustomerEmailsAsync(string searchTerm = null);
}
