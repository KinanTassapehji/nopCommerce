namespace Widgets.FirebasePushNotification.Models;

public class SendBroadcastRequest
{
	public bool SendToAllUsers { get; set; }

	public int CustomerId { get; set; }

	public string Platform { get; set; } = "all";

	public string TitleEn { get; set; } = string.Empty;

	public string BodyEn { get; set; } = string.Empty;

	public string TitleAr { get; set; } = string.Empty;

	public string BodyAr { get; set; } = string.Empty;

	public string DataJson { get; set; } = string.Empty;
}
