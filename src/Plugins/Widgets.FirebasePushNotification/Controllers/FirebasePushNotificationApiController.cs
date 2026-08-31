using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Web.Framework.Controllers;
using Widgets.FirebasePushNotification.Models;
using Widgets.FirebasePushNotification.Services;

namespace Widgets.FirebasePushNotification.Controllers;

[ApiController]
[Route("api/plugin/FirebasePushNotification")]
public class FirebasePushNotificationApiController : BasePluginController
{
	private readonly IFirebaseNotificationService _firebaseNotificationService;

	private readonly IWorkContext _workContext;

	private readonly ICustomerService _customerService;

	private readonly ISettingService _settingService;

	public FirebasePushNotificationApiController(IFirebaseNotificationService firebaseNotificationService, IWorkContext workContext, ICustomerService customerService, ISettingService settingService)
	{
		_firebaseNotificationService = firebaseNotificationService;
		_workContext = workContext;
		_customerService = customerService;
		_settingService = settingService;
	}

	[HttpPost("RegisterToken")]
	public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenRequest request)
	{
		Customer customer = await _workContext.GetCurrentCustomerAsync();
		bool flag = customer == null;
		bool flag2 = flag;
		if (!flag2)
		{
			flag2 = !(await _customerService.IsRegisteredAsync(customer));
		}
		if (flag2)
		{
			return Unauthorized(new
			{
				success = false,
				message = "Authentication required"
			});
		}
		if (request == null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Platform))
		{
			return Ok(new
			{
				success = false,
				message = "Token and platform are required"
			});
		}
		bool success = await _firebaseNotificationService.SubscribeDeviceAsync(customer.Id, request.Token, request.Platform);
		return Ok(new
		{
			success = success,
			message = (success ? "Token registered" : "Invalid platform or token")
		});
	}

	[HttpPost("UnregisterToken")]
	public async Task<IActionResult> UnregisterToken([FromBody] UnregisterTokenRequest request)
	{
		Customer customer = await _workContext.GetCurrentCustomerAsync();
		bool flag = customer == null;
		bool flag2 = flag;
		if (!flag2)
		{
			flag2 = !(await _customerService.IsRegisteredAsync(customer));
		}
		if (flag2)
		{
			return Unauthorized(new
			{
				success = false,
				message = "Authentication required"
			});
		}
		if (request == null || string.IsNullOrWhiteSpace(request.Token))
		{
			return Ok(new
			{
				success = false,
				message = "Token is required"
			});
		}
		bool success = await _firebaseNotificationService.UnsubscribeDeviceAsync(customer.Id, request.Token);
		return Ok(new
		{
			success = success,
			message = (success ? "Token unregistered" : "Unable to unregister token")
		});
	}

	[HttpGet("/firebase-messaging-sw.js")]
	[ResponseCache(NoStore = true, Duration = 0)]
	public async Task<IActionResult> ServiceWorker()
	{
		FirebasePushNotificationSettings settings = await _settingService.LoadSettingAsync<FirebasePushNotificationSettings>();
		string js = $"console.log('[FCM-SW] Service worker loading...');\r\nimportScripts('https://www.gstatic.com/firebasejs/10.14.1/firebase-app-compat.js');\r\nimportScripts('https://www.gstatic.com/firebasejs/10.14.1/firebase-messaging-compat.js');\r\nconsole.log('[FCM-SW] Firebase scripts loaded');\r\n\r\nfirebase.initializeApp({{\r\n  apiKey: '{EscapeJs(settings.ApiKey)}',\r\n  authDomain: '{EscapeJs(settings.AuthDomain)}',\r\n  projectId: '{EscapeJs(settings.ProjectId)}',\r\n  messagingSenderId: '{EscapeJs(settings.MessagingSenderId)}',\r\n  appId: '{EscapeJs(settings.AppId)}'\r\n}});\r\nconsole.log('[FCM-SW] Firebase initialized');\r\n\r\nvar messaging = firebase.messaging();\r\n\r\nmessaging.onBackgroundMessage(function(payload) {{\r\n  console.log('[FCM-SW] Background message received:', payload);\r\n  var n = payload.notification || {{}};\r\n  var d = payload.data || {{}};\r\n  var title = n.title || d.title || 'Notification';\r\n  var options = {{\r\n    body: n.body || d.body || '',\r\n    icon: n.icon || '/icons/icon-192x192.png',\r\n    data: {{ url: d.url || d.click_action || '/' }}\r\n  }};\r\n  console.log('[FCM-SW] Showing notification:', title, options);\r\n  return self.registration.showNotification(title, options);\r\n}});\r\n\r\nself.addEventListener('push', function(event) {{\r\n  console.log('[FCM-SW] Push event received:', event);\r\n  if (!event.data) return;\r\n  try {{\r\n    var payload = event.data.json();\r\n    console.log('[FCM-SW] Push payload:', JSON.stringify(payload));\r\n  }} catch(e) {{\r\n    console.log('[FCM-SW] Push data (text):', event.data.text());\r\n  }}\r\n}});\r\n\r\nself.addEventListener('notificationclick', function(event) {{\r\n  console.log('[FCM-SW] Notification clicked:', event);\r\n  event.notification.close();\r\n  var url = '/';\r\n  if (event.notification.data && event.notification.data.url) {{\r\n    url = event.notification.data.url;\r\n  }}\r\n  event.waitUntil(\r\n    clients.matchAll({{ type: 'window', includeUncontrolled: true }}).then(function(clientList) {{\r\n      for (var i = 0; i < clientList.length; i++) {{\r\n        var client = clientList[i];\r\n        if (client.url.indexOf(self.location.origin) === 0 && 'focus' in client) {{\r\n          client.navigate(url);\r\n          return client.focus();\r\n        }}\r\n      }}\r\n      return clients.openWindow(url);\r\n    }})\r\n  );\r\n}});\r\n\r\nconsole.log('[FCM-SW] Service worker ready');";
		return Content(js, "application/javascript");
	}

	private static string EscapeJs(string value)
	{
		return (value ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n")
			.Replace("\r", "\\r");
	}
}
