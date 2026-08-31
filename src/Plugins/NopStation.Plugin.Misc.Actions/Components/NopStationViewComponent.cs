using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Components;

public abstract class NopStationViewComponent : NopViewComponent
{
	public new IViewComponentResult Content(string content)
	{
		if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(GetType().Assembly).Result)
		{
			return base.Content("");
		}
		return base.Content(content);
	}

	public new IViewComponentResult View()
	{
		if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(GetType().Assembly).Result)
		{
			return base.Content("");
		}
		return base.View();
	}

	public new IViewComponentResult View<TModel>(string viewName, TModel model)
	{
		if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(GetType().Assembly).Result)
		{
			return base.Content("");
		}
		return base.View(viewName, model);
	}

	public new IViewComponentResult View<TModel>(TModel model)
	{
		if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(GetType().Assembly).Result)
		{
			return base.Content("");
		}
		return base.View(model);
	}

	public new IViewComponentResult View(string viewName)
	{
		if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(GetType().Assembly).Result)
		{
			return base.Content("");
		}
		return base.View(viewName);
	}
}
