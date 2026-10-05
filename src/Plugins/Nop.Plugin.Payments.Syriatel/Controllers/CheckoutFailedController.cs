using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Syriatel.Controllers;

public class CheckoutFailedController : BasePluginController
{
    public IActionResult Index(int orderId)
    {
        return View(
            "~/Plugins/Payments.Syriatel/Views/CheckoutFailed/Index.cshtml",
            orderId
        );
    }
}
