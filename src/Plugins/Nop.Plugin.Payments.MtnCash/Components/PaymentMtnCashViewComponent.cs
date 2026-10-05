using Microsoft.AspNetCore.Mvc;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.MtnCash.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.MtnCash.Components;

public class PaymentMtnCashViewComponent : NopViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new PaymentInfoModel()
        {
        };

        //set postback values (we cannot access "Form" with "GET" requests)
        if (!Request.IsGetRequest())
        {
            var form = await Request.ReadFormAsync();

            model.Phone = form["Phone"];
        }

        return View("~/Plugins/Payments.MtnCash/Views/PaymentInfo.cshtml", model);
    }
}