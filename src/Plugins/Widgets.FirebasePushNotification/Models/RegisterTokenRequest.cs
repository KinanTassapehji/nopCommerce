namespace Widgets.FirebasePushNotification.Models;

public class RegisterTokenRequest
{
	public string Token { get; set; } = string.Empty;

	public string Platform { get; set; } = string.Empty;
}
