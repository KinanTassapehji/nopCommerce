using System;
using Nop.Core;

namespace Widgets.FirebasePushNotification.Domain;

/// <summary>
/// A notification as it appears on a notifications page - kept whether or not a device was
/// subscribed to receive the push, so a shopper without push still sees it in the account.
/// </summary>
public class InboxNotification : BaseEntity
{
	/// <summary>
	/// The customer it was sent to; 0 = the store's admin inbox
	/// </summary>
	public int CustomerId { get; set; }

	public string Title { get; set; } = string.Empty;

	public string Body { get; set; } = string.Empty;

	public string? Link { get; set; }

	public bool IsRead { get; set; }

	public DateTime CreatedOnUtc { get; set; }
}
