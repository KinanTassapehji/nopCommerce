using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.MtnCash.Controllers;

public class CheckoutFailedController : BasePluginController
{
    public IActionResult Index(int orderId)
    {
        return View(
            "~/Plugins/Payments.MtnCash/Views/CheckoutFailed/Index.cshtml",
            orderId
        );
    }
}
