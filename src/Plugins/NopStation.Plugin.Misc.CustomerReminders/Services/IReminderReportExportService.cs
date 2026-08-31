using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services;

public interface IReminderReportExportService
{
	Task<string> ExportReminderReportsToXmlAsync(IEnumerable<ReminderReport> reports);

	Task<byte[]> ExportReminderReportsToXlsxAsync(IEnumerable<ReminderReport> reports);
}
