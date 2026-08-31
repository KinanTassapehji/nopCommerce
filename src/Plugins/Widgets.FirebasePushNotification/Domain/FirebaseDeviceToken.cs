using System;
using Nop.Core;

namespace Widgets.FirebasePushNotification.Domain;

public class FirebaseDeviceToken : BaseEntity
{
	public int CustomerId { get; set; }

	public string Token { get; set; } = string.Empty;

	public string Platform { get; set; } = string.Empty;

	public bool IsActive { get; set; }

	public DateTime CreatedOnUtc { get; set; }

	public DateTime UpdatedOnUtc { get; set; }

	public DateTime? LastUsedOnUtc { get; set; }
}
