using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core.Infrastructure;

namespace NopStation.Plugin.Widgets.MegaMenu.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
	private const string THEME_KEY = "nop.themename";

	private const string MEGAMENU_KEY = "nopstation.megamenu";

	public void PopulateValues(ViewLocationExpanderContext context)
	{
		if (context.ViewName == "Components/TopMenu/Default" && EngineContext.Current.Resolve<MegaMenuSettings>().EnableMegaMenu)
		{
			context.Values["nopstation.megamenu"] = "true";
		}
	}

	public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
	{
		string value;
		string value2;
		if (context.AreaName == "Admin")
		{
			viewLocations = new string[2] { "/Plugins/NopStation.Plugin.Widgets.MegaMenu/Areas/Admin/Views/{1}/{0}.cshtml", "/Plugins/NopStation.Plugin.Widgets.MegaMenu/Areas/Admin/Views/Shared/{0}.cshtml" }.Concat(viewLocations);
		}
		else if (context.Values.TryGetValue("nopstation.megamenu", out value))
		{
			viewLocations = new string[1] { "~/Plugins/NopStation.Plugin.Widgets.MegaMenu/Views/Shared/TopMenu.cshtml" };
		}
		else if (context.Values.TryGetValue("nop.themename", out value2))
		{
			viewLocations = new string[2] { "/Plugins/NopStation.Plugin.Widgets.MegaMenu/Views/{1}/{0}.cshtml", "/Plugins/NopStation.Plugin.Widgets.MegaMenu/Views/Shared/{0}.cshtml" }.Concat(viewLocations);
			viewLocations = new string[2]
			{
				"/Plugins/NopStation.Plugin.Widgets.MegaMenu/Themes/" + value2 + "/Views/{1}/{0}.cshtml",
				"/Plugins/NopStation.Plugin.Widgets.MegaMenu/Themes/" + value2 + "/Views/Shared/{0}.cshtml"
			}.Concat(viewLocations);
		}
		return viewLocations;
	}
}
