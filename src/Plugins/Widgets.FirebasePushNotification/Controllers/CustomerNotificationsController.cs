using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Framework.Controllers;
using Widgets.FirebasePushNotification.Services;

namespace Widgets.FirebasePushNotification.Controllers;

/// <summary>
/// My account › Notifications
/// </summary>
public class CustomerNotificationsController : BasePluginController
{
	public const int PageSize = 20;

	private readonly InboxNotificationService _inboxNotificationService;

	private readonly ICustomerService _customerService;

	private readonly IWorkContext _workContext;

	public CustomerNotificationsController(InboxNotificationService inboxNotificationService, ICustomerService customerService, IWorkContext workContext)
	{
		_inboxNotificationService = inboxNotificationService;
		_customerService = customerService;
		_workContext = workContext;
	}

	public async Task<IActionResult> Index(int page = 1)
	{
		var customer = await _workContext.GetCurrentCustomerAsync();
		if (!await _customerService.IsRegisteredAsync(customer))
			return Challenge();

		//loaded before marking, so what was new on arrival still shows as new
		var model = await _inboxNotificationService.GetPageAsync(customer.Id, System.Math.Max(page, 1) - 1, PageSize);
		await _inboxNotificationService.MarkAllReadAsync(customer.Id);

		return View("~/Plugins/Widgets.FirebasePushNotification/Views/CustomerNotifications.cshtml", model);
	}
}
