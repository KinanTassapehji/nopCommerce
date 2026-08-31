using Nop.Core.Caching;

namespace NopStation.Plugin.Misc.CustomerReminders.Infrastructure.Cache;

public static class CustomerRemindersCacheDefaults
{
	public static CacheKey ReminderByIdCacheKey => new CacheKey("NopStation.customerreminders.reminder.byid.{0}");

	public static string ReminderPrefixCacheKey => "NopStation.customerreminders.reminder.";

	public static CacheKey AllEnabledRemindersCacheKey => new CacheKey("NopStation.customerreminders.reminder.allenabled");

	public static CacheKey RemindersByStoreCacheKey => new CacheKey("NopStation.customerreminders.reminder.bystore.{0}-{1}-{2}-{3}");

	public static CacheKey ReminderReportByIdCacheKey => new CacheKey("NopStation.customerreminders.reminderreport.byid.{0}");

	public static string ReminderReportPrefixCacheKey => "NopStation.customerreminders.reminderreport.";

	public static CacheKey ReminderExcludedCustomerByIdCacheKey => new CacheKey("NopStation.customerreminders.excludedcustomer.byid.{0}");

	public static string ReminderExcludedCustomerPrefixCacheKey => "NopStation.customerreminders.excludedcustomer.";

	public static CacheKey IsCustomerExcludedCacheKey => new CacheKey("NopStation.customerreminders.excludedcustomer.isexcluded.{0}-{1}");

	public static CacheKey ExcludedCustomerRecordCacheKey => new CacheKey("NopStation.customerreminders.excludedcustomer.record.{0}-{1}");

	public static CacheKey ExcludedCustomersAllCacheKey => new CacheKey("NopStation.customerreminders.excludedcustomer.all.{0}-{1}-{2}-{3}");
}
