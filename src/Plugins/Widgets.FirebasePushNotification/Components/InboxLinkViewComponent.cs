using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using Widgets.FirebasePushNotification.Services;

namespace Widgets.FirebasePushNotification.Components;

/// <summary>
/// The way into a notifications page: a bell with the unread count in the store header and the
/// admin navbar, and an entry in the account navigation (the only one phones see - the header
/// links give way to the tab bar there)
/// </summary>
public class InboxLinkViewComponent : NopViewComponent
{
	private readonly InboxNotificationService _inboxNotificationService;

	private readonly ICustomerService _customerService;

	private readonly IPermissionService _permissionService;

	private readonly IWorkContext _workContext;

	public InboxLinkViewComponent(InboxNotificationService inboxNotificationService, ICustomerService customerService, IPermissionService permissionService, IWorkContext workContext)
	{
		_inboxNotificationService = inboxNotificationService;
		_customerService = customerService;
		_permissionService = permissionService;
		_workContext = workContext;
	}

	public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
	{
		var customer = await _workContext.GetCurrentCustomerAsync();

		if (widgetZone == AdminWidgetZones.HeaderMiddle)
		{
			if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
				return Content(string.Empty);
			return View("~/Plugins/Widgets.FirebasePushNotification/Views/Components/InboxLink/Admin.cshtml",
				await _inboxNotificationService.CountUnreadAsync(InboxNotificationService.AdminInbox));
		}

		if (!await _customerService.IsRegisteredAsync(customer))
			return Content(string.Empty);

		var view = widgetZone == PublicWidgetZones.AccountNavigationBefore ? "AccountNav" : "Header";
		return View($"~/Plugins/Widgets.FirebasePushNotification/Views/Components/InboxLink/{view}.cshtml",
			await _inboxNotificationService.CountUnreadAsync(customer.Id));
	}
}
