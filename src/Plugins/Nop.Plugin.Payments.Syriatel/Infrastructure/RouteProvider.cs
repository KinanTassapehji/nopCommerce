using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Syriatel.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllerRoute(
            name: "CheckoutFailed",
            pattern: "checkout-failed",
            defaults: new { controller = "CheckoutFailed", action = "Index" }
        );
    }

    public int Priority => 0;
}
