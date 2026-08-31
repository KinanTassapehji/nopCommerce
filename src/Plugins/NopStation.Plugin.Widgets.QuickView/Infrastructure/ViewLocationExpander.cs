using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Services.Cms;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Widgets.QuickView.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
	private const string THEME_KEY = "nop.themename";

	private const string PICTURE_ZOOM_KEY = "nopstation.quickview.picturezoom";

	public void PopulateValues(ViewLocationExpanderContext context)
	{
		if (context != null && context.ControllerName == "QuickView" && context.ViewName == "_ProductDetailsPictures" && NopPlugin.IsEnabledAsync<IWidgetPlugin>("NopStation.Plugin.Widgets.PictureZoom").Result && NopInstance.Load<QuickViewSettings>().EnablePictureZoom)
		{
			context.Values["nopstation.quickview.picturezoom"] = "true";
		}
	}

	public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
	{
		if (context.AreaName == "Admin" || !DisplayZoomPictureView(context))
		{
			return viewLocations;
		}
		viewLocations = new string[1] { "/Plugins/NopStation.Plugin.Widgets.PictureZoom/Views/Shared/PictureZoom.cshtml" }.Concat(viewLocations);
		if (context.Values.TryGetValue("nop.themename", out string value))
		{
			viewLocations = new string[1] { "/Plugins/NopStation.Plugin.Widgets.PictureZoom/Themes/" + value + "/Views/Shared/PictureZoom.cshtml" }.Concat(viewLocations);
		}
		return viewLocations;
	}

	private static bool DisplayZoomPictureView(ViewLocationExpanderContext context)
	{
		if (context.Values.TryGetValue("nopstation.quickview.picturezoom", out string value))
		{
			return value == "true";
		}
		return false;
	}
}
