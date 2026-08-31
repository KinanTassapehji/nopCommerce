using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Infrastructure.Cache;

namespace NopStation.Plugin.Misc.CustomerReminders.Services;

public class ReminderExcludedCustomerService : IReminderExcludedCustomerService
{
	private readonly IRepository<ReminderExcludedCustomer> _reminderExcludedCustomerRepository;

	private readonly IStaticCacheManager _staticCacheManager;

	public ReminderExcludedCustomerService(IRepository<ReminderExcludedCustomer> reminderExcludedCustomerRepository, IStaticCacheManager staticCacheManager)
	{
		_reminderExcludedCustomerRepository = reminderExcludedCustomerRepository;
		_staticCacheManager = staticCacheManager;
	}

	public virtual async Task<ReminderExcludedCustomer> GetReminderExcludedCustomerByIdAsync(int id)
	{
		if (id == 0)
		{
			return null;
		}
		CacheKey key = _staticCacheManager.PrepareKeyForDefaultCache(CustomerRemindersCacheDefaults.ReminderExcludedCustomerByIdCacheKey, id);
		return await _staticCacheManager.GetAsync(key, async () => await _reminderExcludedCustomerRepository.GetByIdAsync(id));
	}

	public virtual async Task<ReminderExcludedCustomer> GetReminderExcludedCustomerAsync(int reminderId, int customerId)
	{
		if (reminderId == 0 || customerId == 0)
		{
			return null;
		}
		CacheKey key = _staticCacheManager.PrepareKeyForDefaultCache(CustomerRemindersCacheDefaults.ExcludedCustomerRecordCacheKey, reminderId, customerId);
		return await _staticCacheManager.GetAsync(key, async () => await _reminderExcludedCustomerRepository.Table.Where((ReminderExcludedCustomer rec) => rec.ReminderId == reminderId && rec.CustomerId == customerId).FirstOrDefaultAsync());
	}

	public virtual async Task<IPagedList<ReminderExcludedCustomer>> GetAllReminderExcludedCustomersAsync(int? reminderId = null, int? customerId = null, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		CacheKey key = _staticCacheManager.PrepareKeyForDefaultCache(CustomerRemindersCacheDefaults.ExcludedCustomersAllCacheKey, reminderId.GetValueOrDefault(), customerId.GetValueOrDefault(), pageIndex, pageSize);
		return await _staticCacheManager.GetAsync(key, async delegate
		{
			IQueryable<ReminderExcludedCustomer> source = _reminderExcludedCustomerRepository.Table;
			if (reminderId.HasValue && reminderId.Value > 0)
			{
				source = source.Where((ReminderExcludedCustomer rec) => rec.ReminderId == ((int?)reminderId).Value);
			}
			if (customerId.HasValue && customerId.Value > 0)
			{
				source = source.Where((ReminderExcludedCustomer rec) => rec.CustomerId == ((int?)customerId).Value);
			}
			source = source.OrderBy((ReminderExcludedCustomer rec) => rec.Id);
			return await source.ToPagedListAsync(pageIndex, pageSize);
		});
	}

	public virtual async Task InsertReminderExcludedCustomerAsync(ReminderExcludedCustomer reminderExcludedCustomer)
	{
		await _reminderExcludedCustomerRepository.InsertAsync(reminderExcludedCustomer);
	}

	public virtual async Task DeleteReminderExcludedCustomerAsync(ReminderExcludedCustomer reminderExcludedCustomer)
	{
		await _reminderExcludedCustomerRepository.DeleteAsync(reminderExcludedCustomer);
	}

	public virtual async Task<bool> IsCustomerExcludedAsync(int reminderId, int customerId)
	{
		if (reminderId == 0 || customerId == 0)
		{
			return false;
		}
		CacheKey key = _staticCacheManager.PrepareKeyForDefaultCache(CustomerRemindersCacheDefaults.IsCustomerExcludedCacheKey, reminderId, customerId);
		return await _staticCacheManager.GetAsync(key, async () => await _reminderExcludedCustomerRepository.Table.Where((ReminderExcludedCustomer rec) => rec.ReminderId == reminderId && rec.CustomerId == customerId).AnyAsync());
	}
}
