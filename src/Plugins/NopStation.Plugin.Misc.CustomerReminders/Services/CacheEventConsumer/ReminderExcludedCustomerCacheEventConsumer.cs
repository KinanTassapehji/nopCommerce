using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Infrastructure.Cache;

namespace NopStation.Plugin.Misc.CustomerReminders.Services.CacheEventConsumer;

public class ReminderExcludedCustomerCacheEventConsumer : CacheEventConsumer<ReminderExcludedCustomer>
{
	protected override async Task ClearCacheAsync(ReminderExcludedCustomer entity)
	{
		await RemoveAsync(CustomerRemindersCacheDefaults.ReminderExcludedCustomerByIdCacheKey, entity.Id);
		await RemoveAsync(CustomerRemindersCacheDefaults.IsCustomerExcludedCacheKey, entity.ReminderId, entity.CustomerId);
		await RemoveAsync(CustomerRemindersCacheDefaults.ExcludedCustomerRecordCacheKey, entity.ReminderId, entity.CustomerId);
		await RemoveByPrefixAsync(CustomerRemindersCacheDefaults.ReminderExcludedCustomerPrefixCacheKey);
	}
}
