using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ClosedXML.Excel;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services;

public class ReminderReportExportService : IReminderReportExportService
{
	public async Task<string> ExportReminderReportsToXmlAsync(IEnumerable<ReminderReport> reports)
	{
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Async = true,
			Encoding = Encoding.UTF8,
			Indent = true
		};
		string result;
		await using (MemoryStream stream = new MemoryStream())
		{
			string text;
			await using (XmlWriter writer = XmlWriter.Create(stream, settings))
			{
				await writer.WriteStartDocumentAsync();
				await writer.WriteStartElementAsync(null, CustomerRemindersDefaults.Export.ReminderReportsRootElement, null);
				foreach (ReminderReport report in reports)
				{
					await writer.WriteStartElementAsync(null, CustomerRemindersDefaults.Export.ReminderReportElement, null);
					await writer.WriteElementStringAsync(null, "Id", null, report.Id.ToString());
					await writer.WriteElementStringAsync(null, "ReminderId", null, report.ReminderId.ToString());
					await writer.WriteElementStringAsync(null, "ReminderName", null, report.ReminderName ?? string.Empty);
					await writer.WriteElementStringAsync(null, "CustomerId", null, report.CustomerId.ToString());
					await writer.WriteElementStringAsync(null, "CustomerName", null, report.CustomerName ?? string.Empty);
					await writer.WriteElementStringAsync(null, "CustomerEmail", null, report.CustomerEmail ?? string.Empty);
					await writer.WriteElementStringAsync(null, "StoreId", null, report.StoreId.ToString());
					await writer.WriteElementStringAsync(null, "StoreName", null, report.StoreName ?? string.Empty);
					await writer.WriteElementStringAsync(null, "IsMessageSent", null, report.IsMessageSent.ToString());
					await writer.WriteElementStringAsync(null, "CreatedOnUtc", null, report.CreatedOnUtc.ToString("O"));
					await writer.WriteEndElementAsync();
				}
				await writer.WriteEndElementAsync();
				await writer.WriteEndDocumentAsync();
				await writer.FlushAsync();
				text = Encoding.UTF8.GetString(stream.ToArray());
			}
			result = text;
		}
		return result;
	}

	public async Task<byte[]> ExportReminderReportsToXlsxAsync(IEnumerable<ReminderReport> reports)
	{
		byte[] result;
		await using (MemoryStream memoryStream = new MemoryStream())
		{
			using (XLWorkbook xLWorkbook = new XLWorkbook())
			{
				IXLWorksheet iXLWorksheet = xLWorkbook.Worksheets.Add(CustomerRemindersDefaults.Export.ReminderReportsWorksheetName);
				iXLWorksheet.Cell(1, 1).Value = CustomerRemindersDefaults.Export.ColumnId;
				iXLWorksheet.Cell(1, 2).Value = CustomerRemindersDefaults.Export.ColumnReminderName;
				iXLWorksheet.Cell(1, 3).Value = CustomerRemindersDefaults.Export.ColumnCustomerName;
				iXLWorksheet.Cell(1, 4).Value = CustomerRemindersDefaults.Export.ColumnCustomerEmail;
				iXLWorksheet.Cell(1, 5).Value = CustomerRemindersDefaults.Export.ColumnStoreName;
				iXLWorksheet.Cell(1, 6).Value = CustomerRemindersDefaults.Export.ColumnIsMessageSent;
				iXLWorksheet.Cell(1, 7).Value = CustomerRemindersDefaults.Export.ColumnCreatedOn;
				int num = 2;
				foreach (ReminderReport report in reports)
				{
					iXLWorksheet.Cell(num, 1).Value = report.Id;
					iXLWorksheet.Cell(num, 2).Value = report.ReminderName ?? string.Empty;
					iXLWorksheet.Cell(num, 3).Value = report.CustomerName ?? string.Empty;
					iXLWorksheet.Cell(num, 4).Value = report.CustomerEmail ?? string.Empty;
					iXLWorksheet.Cell(num, 5).Value = report.StoreName ?? string.Empty;
					iXLWorksheet.Cell(num, 6).Value = (report.IsMessageSent ? "Yes" : "No");
					iXLWorksheet.Cell(num, 7).Value = report.CreatedOnUtc.ToString("G");
					num++;
				}
				iXLWorksheet.Columns().AdjustToContents();
				xLWorkbook.SaveAs(memoryStream);
			}
			result = memoryStream.ToArray();
		}
		return result;
	}
}
