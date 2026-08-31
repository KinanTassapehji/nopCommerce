using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Infrastructure.Cache;

namespace NopStation.Plugin.Misc.CustomerReminders.Services.CacheEventConsumer;

public class ReminderCacheEventConsumer : CacheEventConsumer<Reminder>
{
	protected override async Task ClearCacheAsync(Reminder entity)
	{
		await RemoveAsync(CustomerRemindersCacheDefaults.ReminderByIdCacheKey, entity.Id);
		await RemoveAsync(CustomerRemindersCacheDefaults.AllEnabledRemindersCacheKey);
		await RemoveByPrefixAsync(CustomerRemindersCacheDefaults.ReminderPrefixCacheKey);
	}
}
