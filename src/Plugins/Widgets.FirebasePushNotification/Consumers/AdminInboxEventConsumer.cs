using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Logging;
using Widgets.FirebasePushNotification.Services;

namespace Widgets.FirebasePushNotification.Consumers;

/// <summary>
/// Fills the admin notifications page with the store events the team acts on
/// </summary>
public class AdminInboxEventConsumer : IConsumer<OrderPlacedEvent>, IConsumer<OrderStatusChangedEvent>, IConsumer<CustomerRegisteredEvent>, IConsumer<CustomerAccountClosedEvent>, IConsumer<EntityInsertedEvent<ProductReview>>, IConsumer<EntityInsertedEvent<StockQuantityHistory>>
{
	private readonly InboxNotificationService _inboxNotificationService;

	private readonly ICustomerService _customerService;

	private readonly IProductService _productService;

	private readonly IProductAttributeService _productAttributeService;

	private readonly IPriceFormatter _priceFormatter;

	private readonly ILogger _logger;

	public AdminInboxEventConsumer(InboxNotificationService inboxNotificationService, ICustomerService customerService, IProductService productService, IProductAttributeService productAttributeService, IPriceFormatter priceFormatter, ILogger logger)
	{
		_inboxNotificationService = inboxNotificationService;
		_customerService = customerService;
		_productService = productService;
		_productAttributeService = productAttributeService;
		_priceFormatter = priceFormatter;
		_logger = logger;
	}

	public Task HandleEventAsync(OrderPlacedEvent eventMessage)
	{
		var order = eventMessage.Order;
		return AddAsync("NewOrder", "/Admin/Order/Edit/" + order.Id, async () => new object[]
		{
			order.CustomOrderNumber,
			await _customerService.GetCustomerFullNameAsync(await _customerService.GetCustomerByIdAsync(order.CustomerId)),
			await _priceFormatter.FormatPriceAsync(order.OrderTotal, true, false)
		});
	}

	public Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
	{
		var order = eventMessage.Order;
		if (order.OrderStatus != OrderStatus.Cancelled)
			return Task.CompletedTask;

		return AddAsync("OrderCancelled", "/Admin/Order/Edit/" + order.Id, () => Task.FromResult(new object[] { order.CustomOrderNumber }));
	}

	public Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
	{
		var customer = eventMessage.Customer;
		return AddAsync("NewCustomer", "/Admin/Customer/Edit/" + customer.Id, async () => new object[]
		{
			await _customerService.GetCustomerFullNameAsync(customer),
			customer.Email ?? customer.Username ?? string.Empty
		});
	}

	public Task HandleEventAsync(CustomerAccountClosedEvent eventMessage)
	{
		var customer = eventMessage.Customer;
		return AddAsync("AccountClosed", "/Admin/Customer/Edit/" + customer.Id, async () => new object[]
		{
			await _customerService.GetCustomerFullNameAsync(customer),
			customer.Email ?? customer.Username ?? string.Empty
		});
	}

	public Task HandleEventAsync(EntityInsertedEvent<ProductReview> eventMessage)
	{
		var review = eventMessage.Entity;
		return AddAsync("NewReview", "/Admin/ProductReview/Edit/" + review.Id, async () => new object[]
		{
			(await _productService.GetProductByIdAsync(review.ProductId))?.Name ?? string.Empty,
			review.Rating
		});
	}

	//only when a sale takes stock across the product's "notify admin below" line, not on every
	//sale after it - the store owner email the core sends repeats, an inbox should not.
	//ponytail: the quantity is the one the history row records, per warehouse when several are used
	public async Task HandleEventAsync(EntityInsertedEvent<StockQuantityHistory> eventMessage)
	{
		var history = eventMessage.Entity;
		if (history.QuantityAdjustment >= 0)
			return;

		var threshold = history.CombinationId is int combinationId
			? (await _productAttributeService.GetProductAttributeCombinationByIdAsync(combinationId))?.NotifyAdminForQuantityBelow
			: (await _productService.GetProductByIdAsync(history.ProductId))?.NotifyAdminForQuantityBelow;
		if (threshold is not int below || history.StockQuantity >= below || history.StockQuantity - history.QuantityAdjustment < below)
			return;

		await AddAsync("LowStock", "/Admin/Product/Edit/" + history.ProductId, async () => new object[]
		{
			(await _productService.GetProductByIdAsync(history.ProductId))?.Name ?? string.Empty,
			history.StockQuantity
		});
	}

	//kept as the message key and its arguments: the admin page writes it in the language of
	//whichever admin reads it, not the shopper's who triggered it
	private async Task AddAsync(string key, string link, Func<Task<object[]>> getArgs)
	{
		try
		{
			await _inboxNotificationService.AddAdminMessageAsync(key, link, await getArgs());
		}
		catch (Exception exception)
		{
			await _logger.ErrorAsync($"AdminInboxEventConsumer ({key}) failed to record a notification", exception);
		}
	}
}
