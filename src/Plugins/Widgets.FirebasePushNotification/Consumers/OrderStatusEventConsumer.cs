using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Widgets.FirebasePushNotification.Services;

namespace Widgets.FirebasePushNotification.Consumers;

public class OrderStatusEventConsumer : IConsumer<OrderPlacedEvent>, IConsumer<OrderStatusChangedEvent>, IConsumer<OrderPaidEvent>, IConsumer<ShipmentSentEvent>, IConsumer<ShipmentDeliveredEvent>, IConsumer<ShipmentReadyForPickupEvent>
{
	private readonly IFirebaseNotificationService _firebaseNotificationService;

	private readonly ILocalizationService _localizationService;

	private readonly IOrderService _orderService;

	private readonly ILogger _logger;

	public OrderStatusEventConsumer(IFirebaseNotificationService firebaseNotificationService, ILocalizationService localizationService, IOrderService orderService, ILogger logger)
	{
		_firebaseNotificationService = firebaseNotificationService;
		_localizationService = localizationService;
		_orderService = orderService;
		_logger = logger;
	}

	public Task HandleEventAsync(OrderPlacedEvent eventMessage)
	{
		return NotifyAsync(eventMessage?.Order, "Placed");
	}

	public Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
	{
		var order = eventMessage?.Order;
		var key = order?.OrderStatus switch
		{
			OrderStatus.Processing => "Processing",
			OrderStatus.Complete => "Complete",
			OrderStatus.Cancelled => "Cancelled",
			_ => null
		};
		return key == null ? Task.CompletedTask : NotifyAsync(order, key);
	}

	public Task HandleEventAsync(OrderPaidEvent eventMessage)
	{
		return NotifyAsync(eventMessage?.Order, "Paid");
	}

	public async Task HandleEventAsync(ShipmentSentEvent eventMessage)
	{
		await NotifyAsync(await GetShipmentOrderAsync(eventMessage?.Shipment), "Shipped");
	}

	public async Task HandleEventAsync(ShipmentDeliveredEvent eventMessage)
	{
		await NotifyAsync(await GetShipmentOrderAsync(eventMessage?.Shipment), "Delivered");
	}

	public async Task HandleEventAsync(ShipmentReadyForPickupEvent eventMessage)
	{
		await NotifyAsync(await GetShipmentOrderAsync(eventMessage?.Shipment), "ReadyForPickup");
	}

	private async Task<Order?> GetShipmentOrderAsync(Shipment? shipment)
	{
		return shipment == null ? null : await _orderService.GetOrderByIdAsync(shipment.OrderId);
	}

	//in the language the customer placed the order in, and the tap opens that order
	private async Task NotifyAsync(Order? order, string key)
	{
		try
		{
			if (order == null || order.CustomerId <= 0)
				return;

			var prefix = "Plugins.Widgets.FirebasePushNotification.Order." + key;
			var title = await _localizationService.GetResourceAsync(prefix + ".Title", order.CustomerLanguageId);
			var body = string.Format(await _localizationService.GetResourceAsync(prefix + ".Body", order.CustomerLanguageId), order.CustomOrderNumber);
			var data = new Dictionary<string, string> { ["url"] = "/orderdetails/" + order.Id };
			await _firebaseNotificationService.SendNotificationAsync(order.CustomerId, title, body, data);
		}
		catch (Exception exception)
		{
			await _logger.ErrorAsync($"OrderStatusEventConsumer ({key}) failed to send push notification", exception);
		}
	}
}
