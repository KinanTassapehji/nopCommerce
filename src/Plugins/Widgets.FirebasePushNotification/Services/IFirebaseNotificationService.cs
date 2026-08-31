using System.Collections.Generic;
using System.Threading.Tasks;

namespace Widgets.FirebasePushNotification.Services;

public interface IFirebaseNotificationService
{
	Task<bool> SubscribeDeviceAsync(int customerId, string token, string platform);

	Task<bool> UnsubscribeDeviceAsync(int customerId, string token);

	Task<bool> EnsureCustomerTokenAsync(int customerId, string platform = "web");

	Task<bool> SendNotificationAsync(int customerId, string title, string body, Dictionary<string, string>? data = null, string platform = "all");

	Task<int> SendNotificationToManyAsync(IList<int> customerIds, string title, string body, Dictionary<string, string>? data = null, string platform = "all");
}
