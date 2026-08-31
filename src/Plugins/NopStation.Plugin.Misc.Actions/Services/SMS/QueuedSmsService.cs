using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Services.SMS;

public class QueuedSmsService : IQueuedSmsService
{
	private readonly IRepository<QueuedSms> _queuedSmsRepository;

	public QueuedSmsService(IRepository<QueuedSms> queuedSmsRepository)
	{
		_queuedSmsRepository = queuedSmsRepository;
	}

	public async Task DeleteQueuedSmsAsync(QueuedSms queuedSms)
	{
		await _queuedSmsRepository.DeleteAsync(queuedSms);
	}

	public async Task DeleteAllAsync()
	{
		await _queuedSmsRepository.TruncateAsync();
	}

	public async Task InsertQueuedSmsAsync(QueuedSms queuedSms)
	{
		await _queuedSmsRepository.InsertAsync(queuedSms);
	}

	public async Task UpdateQueuedSmsAsync(QueuedSms queuedSms)
	{
		await _queuedSmsRepository.UpdateAsync(queuedSms);
	}

	public async Task<QueuedSms> GetQueuedSmsByIdAsync(int queuedSmsId)
	{
		if (queuedSmsId == 0)
		{
			return null;
		}
		return await _queuedSmsRepository.GetByIdAsync(queuedSmsId, (ICacheKeyService cache) => (CacheKey)null);
	}

	public async Task<IList<QueuedSms>> GetQueuedSmsByIdsAsync(int[] queuedSmsIds)
	{
		return await _queuedSmsRepository.GetByIdsAsync(queuedSmsIds);
	}

	public async Task<IPagedList<QueuedSms>> GetAllQueuedSmsAsync(bool loadOnlyItemsToBeSent, int maxSentTries = 0, string phoneNumber = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		IQueryable<QueuedSms> source = _queuedSmsRepository.Table;
		if (loadOnlyItemsToBeSent)
		{
			source = source.Where((QueuedSms qe) => !qe.SentOnUtc.HasValue);
		}
		if (maxSentTries > 0)
		{
			source = source.Where((QueuedSms x) => x.SentTries <= maxSentTries);
		}
		if (!string.IsNullOrWhiteSpace(phoneNumber))
		{
			source = source.Where((QueuedSms x) => x.PhoneNumber.Contains(phoneNumber));
		}
		if (createdFromUtc.HasValue)
		{
			source = source.Where((QueuedSms x) => x.CreatedOnUtc >= createdFromUtc);
		}
		if (createdToUtc.HasValue)
		{
			source = source.Where((QueuedSms x) => x.CreatedOnUtc <= createdToUtc);
		}
		source = source.OrderByDescending((QueuedSms e) => e.Id);
		return await source.ToPagedListAsync(pageIndex, pageSize);
	}
}
