namespace Widgets.FirebasePushNotification.Models;

public class FirebaseScriptModel
{
	public string ApiKey { get; set; } = string.Empty;

	public string AuthDomain { get; set; } = string.Empty;

	public string ProjectId { get; set; } = string.Empty;

	public string MessagingSenderId { get; set; } = string.Empty;

	public string AppId { get; set; } = string.Empty;

	public string VapidKey { get; set; } = string.Empty;

	public bool IsAuthenticated { get; set; }

	public int CustomerId { get; set; }
}
