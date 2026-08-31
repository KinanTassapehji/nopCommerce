using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using Widgets.FirebasePushNotification.Models;

namespace Widgets.FirebasePushNotification.Components;

public class FirebaseScriptViewComponent : NopViewComponent
{
	private readonly ISettingService _settingService;

	private readonly IWorkContext _workContext;

	private readonly ICustomerService _customerService;

	public FirebaseScriptViewComponent(ISettingService settingService, IWorkContext workContext, ICustomerService customerService)
	{
		_settingService = settingService;
		_workContext = workContext;
		_customerService = customerService;
	}

	public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
	{
		FirebasePushNotificationSettings settings = await _settingService.LoadSettingAsync<FirebasePushNotificationSettings>();
		if (string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.ProjectId))
		{
			return Content("");
		}
		Customer customer = await _workContext.GetCurrentCustomerAsync();
		bool flag = customer != null;
		bool flag2 = flag;
		if (flag2)
		{
			flag2 = await _customerService.IsRegisteredAsync(customer);
		}
		bool isAuthenticated = flag2;
		FirebaseScriptModel model = new FirebaseScriptModel
		{
			ApiKey = settings.ApiKey,
			AuthDomain = settings.AuthDomain,
			ProjectId = settings.ProjectId,
			MessagingSenderId = settings.MessagingSenderId,
			AppId = settings.AppId,
			VapidKey = settings.VapidKey,
			IsAuthenticated = isAuthenticated,
			CustomerId = (isAuthenticated ? customer.Id : 0)
		};
		return View("~/Plugins/Widgets.FirebasePushNotification/Views/Components/FirebaseScript/Default.cshtml", model);
	}
}
