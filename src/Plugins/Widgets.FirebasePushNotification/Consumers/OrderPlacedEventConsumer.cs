using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using Nop.Services.Logging;
using Widgets.FirebasePushNotification.Services;

namespace Widgets.FirebasePushNotification.Consumers;

public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
{
	private readonly IFirebaseNotificationService _firebaseNotificationService;

	private readonly ILogger _logger;

	public OrderPlacedEventConsumer(IFirebaseNotificationService firebaseNotificationService, ILogger logger)
	{
		_firebaseNotificationService = firebaseNotificationService;
		_logger = logger;
	}

	public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
	{
		try
		{
			Order order = eventMessage?.Order;
			if (order == null || order.CustomerId <= 0)
			{
				return;
			}
			await _firebaseNotificationService.SendNotificationAsync(order.CustomerId, "Order placed", "Your order #" + order.CustomOrderNumber + " has been placed.");
		}
		catch (Exception exception)
		{
			await _logger.ErrorAsync("OrderPlacedEventConsumer failed to send push notification", exception);
		}
	}
}
