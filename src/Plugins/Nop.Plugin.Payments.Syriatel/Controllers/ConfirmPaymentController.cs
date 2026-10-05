using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Syriatel.Interfaces;
using Nop.Plugin.Payments.Syriatel.Models;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Syriatel.Controllers;
[Route("Syriatel")]

public class ConfirmPaymentController : BasePluginController
{
    private readonly ISyriatelService _syriatelService;
    private readonly IOrderService _orderService;
    private readonly ILogger _logger;
    private readonly IWorkContext _workContext;

    public ConfirmPaymentController(
        ISyriatelService syriatelService,
        IOrderService orderService,
        ILogger logger,
        IWorkContext workContext)
    {
        _syriatelService = syriatelService;
        _orderService = orderService;
        _logger = logger;
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
            || order.PaymentStatus != PaymentStatus.Pending || order.PaymentMethodSystemName != "Payments.Syriatel")
        {
            context.Result = RedirectToRoute("Homepage");
            return;
        }

        await next();
    }

    [HttpGet("ConfirmPayment", Name = "SyriatelConfirmPaymentForm")]
    public IActionResult ConfirmPayment(int orderId, string customerMsisdn, string error = null)
    {
        var model = new ConfirmPaymentModel
        {
            OrderId = orderId,
            CustomerMsisdn = customerMsisdn,
            ErrorMessage = error,
            InitFailed = !string.IsNullOrEmpty(error)
        };

        return View(
            "~/Plugins/Payments.Syriatel/Views/ConfirmPayment.cshtml",
            model);
    }

    [HttpPost("Confirm", Name = "SyriatelConfirmPayment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(ConfirmPaymentModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(
                "~/Plugins/Payments.Syriatel/Views/ConfirmPayment.cshtml",
                model);
        }

        try
        {
            await _syriatelService.ConfirmPaymentAsync(
                model.OrderId,
                model.Otp);

            return RedirectToRoute("CheckoutCompleted", new { orderId = model.OrderId });
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Syriatel payment confirmation failed for order #{model.OrderId}", ex);
            model.ErrorMessage = ex.Message;
            return View(
                "~/Plugins/Payments.Syriatel/Views/ConfirmPayment.cshtml",
                model);
        }
    }

    [HttpPost("ResendOtp", Name = "SyriatelResendOtp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(ConfirmPaymentModel model)
    {
        try
        {
            await _syriatelService.ResendOtpAsync(model.OrderId);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Syriatel OTP resend failed for order #{model.OrderId}", ex);
            model.ErrorMessage = ex.Message;
            return View(
                "~/Plugins/Payments.Syriatel/Views/ConfirmPayment.cshtml",
                model);
        }

        return RedirectToAction(nameof(ConfirmPayment), new { orderId = model.OrderId, customerMsisdn = model.CustomerMsisdn });
    }

    [HttpPost("ChangePhone")]
    [ValidateAntiForgeryToken]
    public IActionResult ChangePhone(ConfirmPaymentModel model)
    {
        model.InitFailed = true;
        model.ErrorMessage = null;
        model.Otp = null;

        return View(
            "~/Plugins/Payments.Syriatel/Views/ConfirmPayment.cshtml",
            model);
    }

    [HttpPost("RetryInit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetryInit(ConfirmPaymentModel model)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(model.OrderId)
                ?? throw new Exception("Order not found.");

            await _syriatelService.RequestPaymentAsync(
                model.OrderId,
                model.CustomerMsisdn,
                order.OrderTotal);

            model.ErrorMessage = null;
            model.InitFailed = false;

            return View(
                "~/Plugins/Payments.Syriatel/Views/ConfirmPayment.cshtml",
                model);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Syriatel payment init retry failed for order #{model.OrderId}", ex);
            model.ErrorMessage = ex.Message;
            model.InitFailed = true;
            return View(
                "~/Plugins/Payments.Syriatel/Views/ConfirmPayment.cshtml",
                model);
        }
    }
}
