namespace Widgets.FirebasePushNotification.Models;

public class SendTestRequest
{
	public int CustomerId { get; set; }

	public string Platform { get; set; } = "all";

	public string Title { get; set; } = string.Empty;

	public string Body { get; set; } = string.Empty;

	public string DataJson { get; set; } = string.Empty;
}
