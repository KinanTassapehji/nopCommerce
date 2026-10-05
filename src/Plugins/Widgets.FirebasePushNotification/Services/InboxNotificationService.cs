using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
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

	/// <summary>
	/// Prefix of the admin messages' resources: {prefix}{key}.Title and {prefix}{key}.Body ({0}, {1}... = arguments)
	/// </summary>
	public const string AdminMessagePrefix = "Plugins.Widgets.FirebasePushNotification.Admin.";

	//The admin inbox is read by admins in whatever language their panel is in, so its rows keep
	//the message key (Title) and its arguments as a JSON array (Body), and the page writes the
	//text when it shows it. Customer rows stay plain text: one reader, written in their language.
	public Task AddAdminMessageAsync(string key, string link, params object[] args)
	{
		var arguments = JsonSerializer.Serialize(args.Select(arg => Convert.ToString(arg, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty));
		return AddAsync(new[] { AdminInbox }, key, arguments, link);
	}

	/// <summary>
	/// The message key and arguments of an admin row; null for plain text (rows from before keys)
	/// </summary>
	public static (string Key, string[] Args)? GetAdminMessage(InboxNotification notification)
	{
		if (notification.CustomerId != AdminInbox || !notification.Body.StartsWith('['))
			return null;
		try
		{
			return (notification.Title, JsonSerializer.Deserialize<string[]>(notification.Body) ?? Array.Empty<string>());
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Turns admin rows written as English text (before keys) into key + arguments, by matching
	/// them against each message's English title and body
	/// </summary>
	/// <param name="englishMessages">key -> (English title, English body format)</param>
	public async Task ConvertAdminTextRowsAsync(IDictionary<string, (string Title, string Body)> englishMessages)
	{
		var rows = await _repository.Table
			.Where(x => x.CustomerId == AdminInbox && !x.Body.StartsWith("["))
			.ToListAsync();
		var converted = new List<InboxNotification>();
		foreach (var row in rows)
		{
			foreach (var (key, (title, body)) in englishMessages)
			{
				if (row.Title != title)
					continue;
				//"Order #{0} from {1}, total {2}." -> ^Order\ \#(.*?)\ from\ (.*?),\ total\ (.*?)\.$
				var pattern = "^" + Regex.Replace(Regex.Escape(body), @"\\\{\d+}", "(.*?)") + "$";
				var match = Regex.Match(row.Body, pattern, RegexOptions.Singleline);
				if (!match.Success)
					continue;
				row.Title = key;
				row.Body = JsonSerializer.Serialize(match.Groups.Cast<Group>().Skip(1).Select(group => group.Value));
				converted.Add(row);
				break;
			}
		}
		if (converted.Count > 0)
			await _repository.UpdateAsync(converted, publishEvent: false);
	}

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

	/// <summary>
	/// The admin inbox's filter tabs: group -> the message keys it shows
	/// </summary>
	public static readonly IReadOnlyDictionary<string, string[]> AdminMessageGroups = new Dictionary<string, string[]>
	{
		["orders"] = new[] { "NewOrder", "OrderCancelled" },
		["customers"] = new[] { "NewCustomer", "AccountClosed" },
		["reviews"] = new[] { "NewReview" },
		["stock"] = new[] { "LowStock" }
	};

	/// <param name="keys">admin message keys to keep; null for all</param>
	public Task<IPagedList<InboxNotification>> GetPageAsync(int customerId, int pageIndex, int pageSize, string[]? keys = null)
	{
		return _repository.GetAllPagedAsync(query =>
		{
			query = query.Where(x => x.CustomerId == customerId);
			if (keys != null)
				query = query.Where(x => keys.Contains(x.Title));
			return query.OrderByDescending(x => x.CreatedOnUtc).ThenByDescending(x => x.Id);
		}, pageIndex, pageSize);
	}

	/// <summary>
	/// Unread admin rows per message key, for the counts on the filter tabs
	/// </summary>
	public async Task<Dictionary<string, int>> CountUnreadAdminByKeyAsync()
	{
		return (await _repository.Table
			.Where(x => x.CustomerId == AdminInbox && !x.IsRead)
			.GroupBy(x => x.Title)
			.Select(group => new { group.Key, Count = group.Count() })
			.ToListAsync())
			.ToDictionary(row => row.Key, row => row.Count);
	}

	//ponytail: one indexed COUNT per page view for the bell; cache it if it ever shows in profiling
	public Task<int> CountUnreadAsync(int customerId)
	{
		return _repository.Table.CountAsync(x => x.CustomerId == customerId && !x.IsRead);
	}

	//opening the page reads everything on it, as in most inboxes - no per-item ticking.
	//ponytail: the admin inbox has one read flag shared by every admin; per-admin state if the team grows
	/// <param name="keys">admin message keys to mark (a filter tab); null for all</param>
	public Task MarkAllReadAsync(int customerId, string[]? keys = null)
	{
		var unread = _repository.Table.Where(x => x.CustomerId == customerId && !x.IsRead);
		if (keys != null)
			unread = unread.Where(x => keys.Contains(x.Title));
		return unread
			.Set(x => x.IsRead, true)
			.UpdateAsync();
	}

	private static string Truncate(string value, int max)
	{
		value ??= string.Empty;
		return value.Length <= max ? value : value[..max];
	}
}
