using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Infrastructure.Cache;

namespace NopStation.Plugin.Misc.CustomerReminders.Services;

public class ReminderService : IReminderService
{
	private readonly IRepository<Reminder> _reminderRepository;

	private readonly IStaticCacheManager _staticCacheManager;

	public ReminderService(IRepository<Reminder> reminderRepository, IStaticCacheManager staticCacheManager)
	{
		_reminderRepository = reminderRepository;
		_staticCacheManager = staticCacheManager;
	}

	public virtual async Task<Reminder> GetReminderByIdAsync(int reminderId)
	{
		if (reminderId == 0)
		{
			return null;
		}
		CacheKey key = _staticCacheManager.PrepareKeyForDefaultCache(CustomerRemindersCacheDefaults.ReminderByIdCacheKey, reminderId);
		return await _staticCacheManager.GetAsync(key, async () => await _reminderRepository.GetByIdAsync(reminderId));
	}

	public virtual async Task<IPagedList<Reminder>> GetAllRemindersAsync(string name = null, int storeId = 0, bool? isEnabled = null, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		CacheKey key = _staticCacheManager.PrepareKeyForDefaultCache(CustomerRemindersCacheDefaults.RemindersByStoreCacheKey, name, storeId, isEnabled?.ToString() ?? "null", pageIndex, pageSize);
		return await _staticCacheManager.GetAsync(key, async delegate
		{
			IQueryable<Reminder> source = _reminderRepository.Table;
			if (!string.IsNullOrWhiteSpace(name))
			{
				source = source.Where((Reminder r) => r.Name.Contains(name));
			}
			if (storeId > 0)
			{
				source = source.Where((Reminder r) => r.StoreId == storeId);
			}
			if (isEnabled.HasValue)
			{
				source = source.Where((Reminder r) => r.IsEnabled == ((bool?)isEnabled).Value);
			}
			source = source.Where((Reminder r) => !r.Deleted);
			source = source.OrderBy((Reminder r) => r.Name);
			return await source.ToPagedListAsync(pageIndex, pageSize);
		});
	}

	public virtual async Task InsertReminderAsync(Reminder reminder)
	{
		ArgumentNullException.ThrowIfNull(reminder, "reminder");
		reminder.ExecutedOnUtc = DateTime.UtcNow;
		await _reminderRepository.InsertAsync(reminder);
	}

	public virtual async Task UpdateReminderAsync(Reminder reminder)
	{
		ArgumentNullException.ThrowIfNull(reminder, "reminder");
		await _reminderRepository.UpdateAsync(reminder);
	}

	public virtual async Task DeleteReminderAsync(Reminder reminder)
	{
		reminder.Deleted = true;
		await _reminderRepository.UpdateAsync(reminder);
	}

	public async Task<bool> IsNameUniqueAsync(string name, int currentReminderId = 0)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}
		name = name.Trim();
		return !(await _reminderRepository.Table.Where((Reminder r) => r.Id != currentReminderId && r.Name.Trim().ToLower() == name.ToLower() && !r.Deleted).AnyAsync());
	}

	public virtual async Task<IList<string>> GetDistinctReminderNamesAsync(string searchTerm = null)
	{
		IQueryable<string> source = (from r in _reminderRepository.Table
			where !r.Deleted && !string.IsNullOrEmpty(r.Name)
			select r.Name).Distinct();
		if (!string.IsNullOrWhiteSpace(searchTerm))
		{
			source = source.Where((string name) => name.Contains(searchTerm));
		}
		return await source.OrderBy((string name) => name).Take(10).ToListAsync();
	}
}
