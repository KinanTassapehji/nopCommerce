using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Infrastructure.Cache;

namespace NopStation.Plugin.Misc.CustomerReminders.Services.CacheEventConsumer;

public class ReminderReportCacheEventConsumer : CacheEventConsumer<ReminderReport>
{
	protected override async Task ClearCacheAsync(ReminderReport entity)
	{
		await RemoveAsync(CustomerRemindersCacheDefaults.ReminderReportByIdCacheKey, entity.Id);
		await RemoveByPrefixAsync(CustomerRemindersCacheDefaults.ReminderReportPrefixCacheKey);
	}
}
