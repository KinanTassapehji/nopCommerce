using Microsoft.AspNetCore.Routing;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class CurrentRouteEvent
{
	public RouteValueDictionary RouteValues { get; private set; }

	public string RouteName { get; private set; }

	public CurrentRouteEvent(RouteValueDictionary values)
	{
		RouteValues = values;
	}

	public void SetRouteName(string routeName)
	{
		RouteName = routeName;
	}
}
