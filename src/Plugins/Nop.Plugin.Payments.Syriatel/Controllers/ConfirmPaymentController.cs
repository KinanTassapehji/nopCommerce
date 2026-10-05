using Microsoft.AspNetCore.Mvc;
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

    public ConfirmPaymentController(
        ISyriatelService syriatelService,
        IOrderService orderService,
        ILogger logger)
    {
        _syriatelService = syriatelService;
        _orderService = orderService;
        _logger = logger;
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
