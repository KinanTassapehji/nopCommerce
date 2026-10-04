using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Core;
using Nop.Data;
using Widgets.FirebasePushNotification.Domain;

namespace Widgets.FirebasePushNotification.Services;

/// <summary>
/// The notifications pages' store: one list per customer, and customer 0 for the admin inbox
/// </summary>
public class InboxNotificationService
{
	public const int AdminInbox = 0;

	private readonly IRepository<InboxNotification> _repository;

	public InboxNotificationService(IRepository<InboxNotification> repository)
	{
		_repository = repository;
	}

	public async Task AddAsync(IEnumerable<int> customerIds, string title, string body, string? link = null)
	{
		var now = DateTime.UtcNow;
		var rows = customerIds.Distinct().Select(customerId => new InboxNotification
		{
			CustomerId = customerId,
			Title = Truncate(title, 400),
			Body = Truncate(body, 2000),
			Link = string.IsNullOrWhiteSpace(link) ? null : Truncate(link, 1000),
			CreatedOnUtc = now
		}).ToList();

		if (rows.Count > 0)
			await _repository.InsertAsync(rows, publishEvent: false);
	}

	public Task<IPagedList<InboxNotification>> GetPageAsync(int customerId, int pageIndex, int pageSize)
	{
		return _repository.GetAllPagedAsync(query => query
			.Where(x => x.CustomerId == customerId)
			.OrderByDescending(x => x.CreatedOnUtc).ThenByDescending(x => x.Id), pageIndex, pageSize);
	}

	//ponytail: one indexed COUNT per page view for the bell; cache it if it ever shows in profiling
	public Task<int> CountUnreadAsync(int customerId)
	{
		return _repository.Table.CountAsync(x => x.CustomerId == customerId && !x.IsRead);
	}

	//opening the page reads everything on it, as in most inboxes - no per-item ticking.
	//ponytail: the admin inbox has one read flag shared by every admin; per-admin state if the team grows
	public Task MarkAllReadAsync(int customerId)
	{
		return _repository.Table
			.Where(x => x.CustomerId == customerId && !x.IsRead)
			.Set(x => x.IsRead, true)
			.UpdateAsync();
	}

	private static string Truncate(string value, int max)
	{
		value ??= string.Empty;
		return value.Length <= max ? value : value[..max];
	}
}
