using Nop.Core.Configuration;

namespace Widgets.FirebasePushNotification.Models;

public class FirebasePushNotificationSettings : ISettings
{
	public string ApiKey { get; set; } = string.Empty;

	public string AuthDomain { get; set; } = string.Empty;

	public string ProjectId { get; set; } = string.Empty;

	public string MessagingSenderId { get; set; } = string.Empty;

	public string AppId { get; set; } = string.Empty;

	public string VapidKey { get; set; } = string.Empty;
}
