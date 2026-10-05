using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.MtnCash.Interfaces;
using Nop.Plugin.Payments.MtnCash.Models;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.MtnCash.Controllers;
[Route("MtnCash")]

public class ConfirmPaymentController : BasePluginController
{
    private readonly IMtnCashService _mtnCashService;
    private readonly ILogger _logger;
    private readonly IOrderService _orderService;
    private readonly IWorkContext _workContext;

    public ConfirmPaymentController(IMtnCashService mtnCashService, ILogger logger, IOrderService orderService, IWorkContext workContext)
    {
        _mtnCashService = mtnCashService;
        _logger = logger;
        _orderService = orderService;
        _workContext = workContext;
    }

    /// <summary>
    /// Every action here acts on one order taken from the request; only its owner may touch it,
    /// and only while it still awaits this payment method
    /// </summary>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var orderId = context.ActionArguments.Values.Select(arg => arg switch
        {
            int id => id,
            ConfirmPaymentModel model => model.OrderId,
            _ => 0
        }).FirstOrDefault(id => id > 0);

        var order = await _orderService.GetOrderByIdAsync(orderId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (order is null || order.Deleted || order.CustomerId != customer.Id
            || order.PaymentStatus != PaymentStatus.Pending || order.PaymentMethodSystemName != "Payments.MtnCash")
        {
            context.Result = RedirectToRoute("Homepage");
            return;
        }

        await next();
    }

    [HttpGet("ConfirmPayment")]
    public IActionResult ConfirmPayment(int orderId, string phone, string error = null)
    {
        var model = new ConfirmPaymentModel
        {
            OrderId = orderId,
            Phone = phone,
            ErrorMessage = error,
            InitFailed = !string.IsNullOrEmpty(error)
        };

        return View(
            "~/Plugins/Payments.MtnCash/Views/ConfirmPayment.cshtml",
            model);
    }

    [HttpPost("RetryInit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetryInit(ConfirmPaymentModel model)
    {
        try
        {
            await _mtnCashService.InitMtnPayment(model.OrderId, model.Phone);

            model.ErrorMessage = null;
            model.InitFailed = false;

            return View(
                "~/Plugins/Payments.MtnCash/Views/ConfirmPayment.cshtml",
                model);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"MTN Cash payment init retry failed for order #{model.OrderId}", ex);
            model.ErrorMessage = ex.Message;
            model.InitFailed = true;
            return View(
                "~/Plugins/Payments.MtnCash/Views/ConfirmPayment.cshtml",
                model);
        }
    }

    [HttpPost("Confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(ConfirmPaymentModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(
                "~/Plugins/Payments.MtnCash/Views/ConfirmPayment.cshtml",
                model);
        }

        try
        {
            await _mtnCashService.ConifermMtnPayment(
                model.Phone,
                model.OrderId,
                model.Code);

            return RedirectToRoute("CheckoutCompleted", new { orderId = model.OrderId });
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"MTN Cash payment confirmation failed for order #{model.OrderId}", ex);
            model.ErrorMessage = ex.Message;
            return View(
                "~/Plugins/Payments.MtnCash/Views/ConfirmPayment.cshtml",
                model);
        }
    }

    [HttpPost("ChangePhone")]
    [ValidateAntiForgeryToken]
    public IActionResult ChangePhone(ConfirmPaymentModel model)
    {
        model.InitFailed = true;
        model.ErrorMessage = null;
        model.Code = null;

        return View(
            "~/Plugins/Payments.MtnCash/Views/ConfirmPayment.cshtml",
            model);
    }
}
