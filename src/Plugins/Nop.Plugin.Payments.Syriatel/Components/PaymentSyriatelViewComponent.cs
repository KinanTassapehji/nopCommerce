using Microsoft.AspNetCore.Mvc;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.Syriatel.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Syriatel.Components;

public class PaymentSyriatelViewComponent : NopViewComponent
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

            model.CustomerMsisdn = form["CustomerMsisdn"];
        }

        return View("~/Plugins/Payments.Syriatel/Views/PaymentInfo.cshtml", model);
    }
}
