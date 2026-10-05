using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.MtnCash.Interfaces;
using Nop.Plugin.Payments.MtnCash.Models;
using Nop.Services.Logging;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.MtnCash.Controllers;
[Route("MtnCash")]

public class ConfirmPaymentController : BasePluginController
{
    private readonly IMtnCashService _mtnCashService;
    private readonly ILogger _logger;

    public ConfirmPaymentController(IMtnCashService mtnCashService, ILogger logger)
    {
        _mtnCashService = mtnCashService;
        _logger = logger;
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
