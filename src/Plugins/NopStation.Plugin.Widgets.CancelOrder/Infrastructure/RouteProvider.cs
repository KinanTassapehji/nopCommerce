using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.CancelOrder.Infrastructure;

public class RouteProvider : BaseRouteProvider, IRouteProvider
{
	public int Priority => 100;

	public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
	{
		string languageRoutePattern = GetLanguageRoutePattern();
		endpointRouteBuilder.MapControllerRoute("CustomerCancelOrder", languageRoutePattern + "cancel-order", new
		{
			controller = "CancelOrder",
			action = "Cancel"
		});
	}
}
