using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.QuickView.Infrastructure;

public class RouteProvider : BaseRouteProvider, IRouteProvider
{
	public int Priority => 10;

	public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
	{
		string languageRoutePattern = GetLanguageRoutePattern();
		endpointRouteBuilder.MapControllerRoute("QuickViewProductDetails", languageRoutePattern + "quickview-product-details", new
		{
			controller = "QuickView",
			action = "ProductDetails"
		});
	}
}
