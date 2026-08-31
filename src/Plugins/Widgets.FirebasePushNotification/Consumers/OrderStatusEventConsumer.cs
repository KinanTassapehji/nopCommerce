using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Widgets.FirebasePushNotification.Services;

namespace Widgets.FirebasePushNotification.Consumers;

public class OrderStatusEventConsumer : IConsumer<OrderStatusChangedEvent>, IConsumer<OrderPaidEvent>, IConsumer<ShipmentSentEvent>, IConsumer<ShipmentDeliveredEvent>, IConsumer<ShipmentReadyForPickupEvent>
{
	private readonly IFirebaseNotificationService _firebaseNotificationService;

	private readonly IOrderService _orderService;

	private readonly ILogger _logger;

	public OrderStatusEventConsumer(IFirebaseNotificationService firebaseNotificationService, IOrderService orderService, ILogger logger)
	{
		_firebaseNotificationService = firebaseNotificationService;
		_orderService = orderService;
		_logger = logger;
	}

	public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
	{
		try
		{
			Order order = eventMessage?.Order;
			if (order == null || order.CustomerId <= 0)
			{
				return;
			}
			OrderStatus orderStatus = order.OrderStatus;
			if (1 == 0)
			{
			}
			(string, string) tuple = orderStatus switch
			{
				OrderStatus.Processing => ("Order processing", "Your order #" + order.CustomOrderNumber + " is being processed."), 
				OrderStatus.Complete => ("Order complete", "Your order #" + order.CustomOrderNumber + " has been completed."), 
				OrderStatus.Cancelled => ("Order cancelled", "Your order #" + order.CustomOrderNumber + " has been cancelled."), 
				_ => (null, null), 
			};
			if (1 == 0)
			{
			}
			var (title, body) = tuple;
			if (title == null)
			{
				return;
			}
			await _firebaseNotificationService.SendNotificationAsync(order.CustomerId, title, body);
		}
		catch (Exception exception)
		{
			await _logger.ErrorAsync("OrderStatusEventConsumer failed to send push notification", exception);
		}
	}

	public async Task HandleEventAsync(OrderPaidEvent eventMessage)
	{
		try
		{
			Order order = eventMessage?.Order;
			if (order == null || order.CustomerId <= 0)
			{
				return;
			}
			await _firebaseNotificationService.SendNotificationAsync(order.CustomerId, "Payment confirmed", "Payment for order #" + order.CustomOrderNumber + " has been confirmed.");
		}
		catch (Exception exception)
		{
			await _logger.ErrorAsync("OrderStatusEventConsumer (paid) failed to send push notification", exception);
		}
	}

	public async Task HandleEventAsync(ShipmentSentEvent eventMessage)
	{
		await HandleShipmentEventAsync(eventMessage?.Shipment, "Order shipped", "Your order #{0} has been shipped.");
	}

	public async Task HandleEventAsync(ShipmentDeliveredEvent eventMessage)
	{
		await HandleShipmentEventAsync(eventMessage?.Shipment, "Order delivered", "Your order #{0} has been delivered.");
	}

	public async Task HandleEventAsync(ShipmentReadyForPickupEvent eventMessage)
	{
		await HandleShipmentEventAsync(eventMessage?.Shipment, "Ready for pickup", "Your order #{0} is ready for pickup.");
	}

	private async Task HandleShipmentEventAsync(Shipment shipment, string title, string bodyTemplate)
	{
		try
		{
			if (shipment == null)
			{
				return;
			}
			Order order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
			if (order == null || order.CustomerId <= 0)
			{
				return;
			}
			string body = string.Format(bodyTemplate, order.CustomOrderNumber);
			await _firebaseNotificationService.SendNotificationAsync(order.CustomerId, title, body);
		}
		catch (Exception exception)
		{
			await _logger.ErrorAsync("OrderStatusEventConsumer (" + title + ") failed to send push notification", exception);
		}
	}
}
