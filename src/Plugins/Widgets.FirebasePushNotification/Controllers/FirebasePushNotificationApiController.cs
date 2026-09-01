using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
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

	public FirebasePushNotificationApiController(IFirebaseNotificationService firebaseNotificationService, IWorkContext workContext, ICustomerService customerService)
	{
		_firebaseNotificationService = firebaseNotificationService;
		_workContext = workContext;
		_customerService = customerService;
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
}
