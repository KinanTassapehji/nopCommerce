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

public class ReminderReportService : IReminderReportService
{
	private readonly IRepository<ReminderReport> _reminderReportRepository;

	private readonly IStaticCacheManager _staticCacheManager;

	public ReminderReportService(IRepository<ReminderReport> reminderReportRepository, IStaticCacheManager staticCacheManager)
	{
		_reminderReportRepository = reminderReportRepository;
		_staticCacheManager = staticCacheManager;
	}

	public virtual async Task<ReminderReport> GetReminderReportByIdAsync(int id)
	{
		if (id == 0)
		{
			return null;
		}
		CacheKey key = _staticCacheManager.PrepareKeyForDefaultCache(CustomerRemindersCacheDefaults.ReminderReportByIdCacheKey, id);
		return await _staticCacheManager.GetAsync(key, async () => await _reminderReportRepository.GetByIdAsync(id));
	}

	public virtual async Task<IPagedList<ReminderReport>> GetAllReminderReportsAsync(int? reminderId = null, string reminderName = null, int? customerId = null, string customerName = null, string customerEmail = null, int? storeId = null, string storeName = null, bool? isMessageSent = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		IQueryable<ReminderReport> source = _reminderReportRepository.Table;
		if (reminderId.HasValue && reminderId.Value > 0)
		{
			source = source.Where((ReminderReport rr) => rr.ReminderId == (int?)((int?)reminderId).Value);
		}
		if (!string.IsNullOrWhiteSpace(reminderName))
		{
			source = source.Where((ReminderReport rr) => rr.ReminderName.Contains(reminderName));
		}
		if (customerId.HasValue && customerId.Value > 0)
		{
			source = source.Where((ReminderReport rr) => rr.CustomerId == (int?)((int?)customerId).Value);
		}
		if (!string.IsNullOrWhiteSpace(customerName))
		{
			source = source.Where((ReminderReport rr) => rr.CustomerName.Contains(customerName));
		}
		if (!string.IsNullOrWhiteSpace(customerEmail))
		{
			source = source.Where((ReminderReport rr) => rr.CustomerEmail.Contains(customerEmail));
		}
		if (storeId.HasValue && storeId.Value > 0)
		{
			source = source.Where((ReminderReport rr) => rr.StoreId == ((int?)storeId).Value);
		}
		if (!string.IsNullOrWhiteSpace(storeName))
		{
			source = source.Where((ReminderReport rr) => rr.StoreName.Contains(storeName));
		}
		if (isMessageSent.HasValue)
		{
			source = source.Where((ReminderReport rr) => rr.IsMessageSent == ((bool?)isMessageSent).Value);
		}
		if (createdFromUtc.HasValue)
		{
			source = source.Where((ReminderReport rr) => rr.CreatedOnUtc >= ((DateTime?)createdFromUtc).Value);
		}
		if (createdToUtc.HasValue)
		{
			source = source.Where((ReminderReport rr) => rr.CreatedOnUtc <= ((DateTime?)createdToUtc).Value);
		}
		source = source.OrderByDescending((ReminderReport rr) => rr.CreatedOnUtc);
		return await source.ToPagedListAsync(pageIndex, pageSize);
	}

	public virtual async Task<int> GetSentMessageCountAsync(int customerId, int reminderId)
	{
		return await _reminderReportRepository.Table.CountAsync((ReminderReport rr) => rr.CustomerId == (int?)customerId && rr.ReminderId == (int?)reminderId && rr.IsMessageSent);
	}

	public virtual async Task<ReminderReport> GetLastSentReportAsync(int customerId, int reminderId)
	{
		return await (from rr in _reminderReportRepository.Table
			where rr.CustomerId == (int?)customerId && rr.ReminderId == (int?)reminderId && rr.IsMessageSent
			orderby rr.CreatedOnUtc descending
			select rr).FirstOrDefaultAsync();
	}

	public virtual async Task InsertReminderReportAsync(ReminderReport reminderReport)
	{
		ArgumentNullException.ThrowIfNull(reminderReport, "reminderReport");
		await _reminderReportRepository.InsertAsync(reminderReport);
	}

	public virtual async Task DeleteReminderReportAsync(ReminderReport reminderReport)
	{
		ArgumentNullException.ThrowIfNull(reminderReport, "reminderReport");
		await _reminderReportRepository.DeleteAsync(reminderReport);
	}

	public virtual async Task<IList<ReminderReport>> GetReminderReportsByIdsAsync(int[] ids)
	{
		if (ids == null || ids.Length == 0)
		{
			return new List<ReminderReport>();
		}
		return await _reminderReportRepository.Table.Where((ReminderReport rr) => ids.Contains(rr.Id)).ToListAsync();
	}

	public virtual async Task DeleteReminderReportsAsync(IList<ReminderReport> reminderReports)
	{
		ArgumentNullException.ThrowIfNull(reminderReports, "reminderReports");
		await _reminderReportRepository.DeleteAsync(reminderReports);
	}

	public virtual async Task<IList<string>> GetDistinctReminderNamesAsync(string searchTerm = null)
	{
		IQueryable<string> source = (from rr in _reminderReportRepository.Table
			where !string.IsNullOrEmpty(rr.ReminderName)
			select rr.ReminderName).Distinct();
		if (!string.IsNullOrWhiteSpace(searchTerm))
		{
			source = source.Where((string name) => name.Contains(searchTerm));
		}
		return await source.OrderBy((string name) => name).Take(10).ToListAsync();
	}

	public virtual async Task<IList<string>> GetDistinctCustomerNamesAsync(string searchTerm = null)
	{
		IQueryable<string> source = (from rr in _reminderReportRepository.Table
			where !string.IsNullOrEmpty(rr.CustomerName)
			select rr.CustomerName).Distinct();
		if (!string.IsNullOrWhiteSpace(searchTerm))
		{
			source = source.Where((string name) => name.Contains(searchTerm));
		}
		return await source.OrderBy((string name) => name).Take(10).ToListAsync();
	}

	public virtual async Task<IList<string>> GetDistinctCustomerEmailsAsync(string searchTerm = null)
	{
		IQueryable<string> source = (from rr in _reminderReportRepository.Table
			where !string.IsNullOrEmpty(rr.CustomerEmail)
			select rr.CustomerEmail).Distinct();
		if (!string.IsNullOrWhiteSpace(searchTerm))
		{
			source = source.Where((string email) => email.Contains(searchTerm));
		}
		return await source.OrderBy((string email) => email).Take(10).ToListAsync();
	}
}
