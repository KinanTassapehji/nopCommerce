using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Widgets.CancelOrder.Controllers;

public class CancelOrderController : NopStationPublicController
{
	private readonly IOrderService _orderService;

	private readonly IWorkContext _workContext;

	private readonly CancelOrderSettings _cancelOrderSettings;

	private readonly INotificationService _notificationService;

	private readonly ILocalizationService _localizationService;

	public CancelOrderController(IOrderService orderService, IWorkContext workContext, CancelOrderSettings cancelOrderSettings, INotificationService notificationService, ILocalizationService localizationService)
	{
		_orderService = orderService;
		_workContext = workContext;
		_cancelOrderSettings = cancelOrderSettings;
		_notificationService = notificationService;
		_localizationService = localizationService;
	}

	[HttpPost]
	public async Task<IActionResult> Cancel(int orderId)
	{
		Order order = await _orderService.GetOrderByIdAsync(orderId);
		if (order == null || order.Deleted)
		{
			INotificationService notificationService = _notificationService;
			notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.CancelOrder.OrderNotFound"));
			return Json(new
			{
				Result = false
			});
		}
		int customerId = order.CustomerId;
		if (customerId != (await _workContext.GetCurrentCustomerAsync()).Id)
		{
			INotificationService notificationService = _notificationService;
			notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.CancelOrder.InvalidRequest"));
			return Json(new
			{
				Result = false
			});
		}
		if (!_cancelOrderSettings.CancellableOrderStatuses.Contains(order.OrderStatusId) || !_cancelOrderSettings.CancellablePaymentStatuses.Contains(order.PaymentStatusId) || !_cancelOrderSettings.CancellableShippingStatuses.Contains(order.ShippingStatusId))
		{
			INotificationService notificationService = _notificationService;
			notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.CancelOrder.InvalidRequest"));
			return Json(new
			{
				Result = false
			});
		}
		order.OrderStatusId = 40;
		await _orderService.UpdateOrderAsync(order);
		OrderNote orderNote = new OrderNote
		{
			OrderId = order.Id,
			CreatedOnUtc = DateTime.UtcNow,
			DisplayToCustomer = false
		};
		OrderNote orderNote2 = orderNote;
		orderNote2.Note = await _localizationService.GetResourceAsync("NopStation.CancelOrder.OrderCancelledByCustomer");
		await _orderService.InsertOrderNoteAsync(orderNote);
		return Json(new
		{
			Result = true
		});
	}
}
