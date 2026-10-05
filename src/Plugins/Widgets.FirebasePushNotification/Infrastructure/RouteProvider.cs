using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Widgets.FirebasePushNotification.Infrastructure;

public class RouteProvider : BaseRouteProvider, IRouteProvider
{
	public const string NotificationsRouteName = "Plugin.Widgets.FirebasePushNotification.Notifications";

	public int Priority => 0;

	public void RegisterRoutes(IEndpointRouteBuilder endpoints)
	{
		endpoints.MapControllerRoute(NotificationsRouteName, $"{GetLanguageRoutePattern()}/customer/notifications", new
		{
			controller = "CustomerNotifications",
			action = "Index"
		});
		endpoints.MapControllerRoute("Plugin.Widgets.FirebasePushNotification.AdminInbox", "Admin/FirebasePushNotification/Inbox", new
		{
			controller = "FirebasePushNotification",
			action = "Inbox"
		});
		endpoints.MapControllerRoute("Plugin.Widgets.FirebasePushNotification.Configure", "Admin/FirebasePushNotification/Configure", new
		{
			controller = "FirebasePushNotification",
			action = "Configure"
		});
		endpoints.MapControllerRoute("Plugin.Widgets.FirebasePushNotification.Configure.Legacy", "Admin/FirebasePushNotificationAdmin/Configure", new
		{
			controller = "FirebasePushNotification",
			action = "Configure"
		});
		endpoints.MapControllerRoute("Plugin.Widgets.FirebasePushNotification.SendBroadcast", "Admin/FirebasePushNotification/SendBroadcast", new
		{
			controller = "FirebasePushNotification",
			action = "SendBroadcast"
		});
	}
}
