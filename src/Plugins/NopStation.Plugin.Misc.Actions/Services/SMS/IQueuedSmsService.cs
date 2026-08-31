using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Services.SMS;

public interface IQueuedSmsService
{
	Task DeleteQueuedSmsAsync(QueuedSms queuedSms);

	Task DeleteAllAsync();

	Task InsertQueuedSmsAsync(QueuedSms queuedSms);

	Task UpdateQueuedSmsAsync(QueuedSms queuedSms);

	Task<QueuedSms> GetQueuedSmsByIdAsync(int queuedSmsId);

	Task<IList<QueuedSms>> GetQueuedSmsByIdsAsync(int[] queuedSmsIds);

	Task<IPagedList<QueuedSms>> GetAllQueuedSmsAsync(bool loadOnlyItemsToBeSent, int maxSentTries = 0, string phoneNumber = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue);
}
