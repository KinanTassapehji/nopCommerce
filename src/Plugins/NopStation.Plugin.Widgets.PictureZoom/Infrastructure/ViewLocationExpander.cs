using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Widgets.PictureZoom.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
	private const string THEME_KEY = "nop.themename";

	private const string PICTURE_ZOOM_KEY = "nopstation.quickview.picturezoom";

	private static bool DisplayZoomPictureView(ViewLocationExpanderContext context)
	{
		if (context.Values.TryGetValue("nopstation.quickview.picturezoom", out string value))
		{
			return value == "true";
		}
		return false;
	}

	public void PopulateValues(ViewLocationExpanderContext context)
	{
		if (!(context.ControllerName != "Product") && !(context.ViewName != "_ProductDetailsPictures"))
		{
			IPluginService pluginService = NopInstance.Load<IPluginService>();
			IStoreContext storeContext = NopInstance.Load<IStoreContext>();
			IWorkContext workContext = NopInstance.Load<IWorkContext>();
			IWidgetPluginManager widgetPluginManager = NopInstance.Load<IWidgetPluginManager>();
			PictureZoomSettings pictureZoomSettings = EngineContext.Current.Resolve<PictureZoomSettings>();
			PluginDescriptor result = pluginService.GetPluginDescriptorBySystemNameAsync<IWidgetPlugin>("NopStation.Plugin.Widgets.PictureZoom", LoadPluginsMode.InstalledOnly, workContext.GetCurrentCustomerAsync().Result, storeContext.GetCurrentStoreAsync().Result.Id).Result;
			if (result != null && widgetPluginManager.IsPluginActive(result.Instance<IWidgetPlugin>()) && pictureZoomSettings.EnablePictureZoom)
			{
				context.Values["nopstation.quickview.picturezoom"] = "true";
			}
		}
	}

	public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
	{
		if (context.AreaName == "Admin" || !DisplayZoomPictureView(context))
		{
			return viewLocations;
		}
		viewLocations = new string[1] { "/Plugins/NopStation.Plugin.Widgets.PictureZoom/Views/Shared/PictureZoom.cshtml" };
		if (context.Values.TryGetValue("nop.themename", out string value))
		{
			viewLocations = new string[1] { "/Plugins/NopStation.Plugin.Widgets.PictureZoom/Themes/" + value + "/Views/Shared/PictureZoom.cshtml" }.Concat(viewLocations);
		}
		return viewLocations;
	}
}
