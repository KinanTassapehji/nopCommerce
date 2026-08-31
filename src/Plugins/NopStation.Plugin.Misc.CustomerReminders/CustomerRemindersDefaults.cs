namespace NopStation.Plugin.Misc.CustomerReminders;

public static class CustomerRemindersDefaults
{
	public class Route
	{
		public static string Configuration => "NopStation.Plugin.CustomerReminders.Configure";

		public static string ReminderRules => "NopStation.Plugin.CustomerReminders.ReminderRules";

		public static string Reminders => "NopStation.Plugin.CustomerReminders.Reminders";

		public static string ReminderReports => "NopStation.Plugin.CustomerReminders.ReminderReports";
	}

	public class Export
	{
		public static string ReminderReportsXmlFileName => "reminderreports.xml";

		public static string ReminderReportsXlsxFileName => "reminderreports.xlsx";

		public static string ReminderReportsWorksheetName => "ReminderReports";

		public static string ReminderReportsRootElement => "ReminderReports";

		public static string ReminderReportElement => "ReminderReport";

		public static string ColumnId => "Id";

		public static string ColumnReminderName => "Reminder Name";

		public static string ColumnCustomerName => "Customer Name";

		public static string ColumnCustomerEmail => "Customer Email";

		public static string ColumnStoreName => "Store Name";

		public static string ColumnIsMessageSent => "Is Message Sent";

		public static string ColumnCreatedOn => "Created On";
	}

	public static string PluginSystemName => "Misc.CustomerReminders";

	public static string PluginMenuSystemName => "NopStation.AdminMenu.CustomerReminders";

	public static string TableNamePrefix => "NS_";
}
