using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Widgets.FirebasePushNotification.Infrastructure;

public class RouteProvider : IRouteProvider
{
	public int Priority => 0;

	public void RegisterRoutes(IEndpointRouteBuilder endpoints)
	{
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
