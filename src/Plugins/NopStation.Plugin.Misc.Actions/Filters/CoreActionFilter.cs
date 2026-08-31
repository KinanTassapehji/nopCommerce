using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Filters;

public class CoreActionFilter : IActionFilter, IFilterMetadata
{
	public void OnActionExecuted(ActionExecutedContext context)
	{
	}

	public void OnActionExecuting(ActionExecutingContext context)
	{
		if (context == null)
		{
			return;
		}
		ControllerActionDescriptor controllerActionDescriptor = ((ControllerBase)context.Controller).ControllerContext?.ActionDescriptor;
		Assembly assembly = controllerActionDescriptor.ControllerTypeInfo.Assembly;
		if (assembly.GetName().Name.StartsWith("NopStation.Plugin.", StringComparison.InvariantCultureIgnoreCase) && (!controllerActionDescriptor.ControllerName.Equals("NopStationLicense", StringComparison.InvariantCultureIgnoreCase) || !controllerActionDescriptor.ActionName.Equals("License", StringComparison.InvariantCultureIgnoreCase)) && !NopInstance.Load<ILicenseService>().IsLicensedAsync(assembly).Result)
		{
			string text = string.Empty;
			RouteData routeData = ((ControllerBase)context.Controller).ControllerContext.RouteData;
			if (routeData != null)
			{
				text = routeData.Values["area"] as string;
			}
			if (text != null && text.Equals("admin", StringComparison.InvariantCultureIgnoreCase))
			{
				context.Result = new RedirectToActionResult("License", "NopStationLicense", null);
			}
			else
			{
				context.Result = new RedirectToActionResult("Index", "Home", null);
			}
		}
	}
}
