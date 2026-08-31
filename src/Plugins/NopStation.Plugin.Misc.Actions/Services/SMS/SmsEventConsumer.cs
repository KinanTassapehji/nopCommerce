using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Forums;
using Nop.Services.Orders;
using Nop.Services.Vendors;

namespace NopStation.Plugin.Misc.Core.Services.SMS;

public class SmsEventConsumer : IConsumer<OrderStatusChangedEvent>, IConsumer<CustomerRegisteredEvent>, IConsumer<CustomerActivatedEvent>, IConsumer<OrderPlacedEvent>, IConsumer<OrderPaidEvent>, IConsumer<ShipmentSentEvent>, IConsumer<ShipmentDeliveredEvent>, IConsumer<OrderRefundedEvent>, IConsumer<EntityInsertedEvent<ForumTopic>>, IConsumer<EntityInsertedEvent<ForumPost>>, IConsumer<EntityInsertedEvent<PrivateMessage>>
{
	private readonly IWorkContext _workContext;

	private readonly IWorkflowSmsService _workflowSmsService;

	private readonly LocalizationSettings _localizationSettings;

	private readonly CustomerSettings _customerSettings;

	private readonly IVendorService _vendorService;

	private readonly ICustomerService _customerService;

	private readonly IForumService _forumService;

	private readonly ForumSettings _forumSettings;

	private readonly IOrderService _orderService;

	private readonly IProductService _productService;

	public SmsEventConsumer(IWorkContext workContext, IWorkflowSmsService workflowSmsService, LocalizationSettings localizationSettings, CustomerSettings customerSettings, IVendorService vendorService, ICustomerService customerService, IForumService forumService, ForumSettings forumSettings, IOrderService orderService, IProductService productService)
	{
		_workContext = workContext;
		_workflowSmsService = workflowSmsService;
		_localizationSettings = localizationSettings;
		_customerSettings = customerSettings;
		_vendorService = vendorService;
		_customerService = customerService;
		_forumService = forumService;
		_forumSettings = forumSettings;
		_orderService = orderService;
		_productService = productService;
	}

	protected virtual async Task<IList<Vendor>> GetVendorsInOrderAsync(Order order)
	{
		List<Vendor> vendors = new List<Vendor>();
		foreach (OrderItem item in await _orderService.GetOrderItemsAsync(order.Id))
		{
			int vendorId = (await _productService.GetProductByIdAsync(item.ProductId)).VendorId;
			Vendor vendor = vendors.FirstOrDefault((Vendor v) => v.Id == vendorId);
			if (vendor == null)
			{
				vendor = await _vendorService.GetVendorByIdAsync(vendorId);
				if (vendor != null && !vendor.Deleted && vendor.Active)
				{
					vendors.Add(vendor);
				}
			}
		}
		return vendors;
	}

	public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
	{
		OrderStatus prevOrderStatus = eventMessage.PreviousOrderStatus;
		OrderStatus os = eventMessage.Order.OrderStatus;
		if (prevOrderStatus != os)
		{
			if (prevOrderStatus != OrderStatus.Complete && os == OrderStatus.Complete)
			{
				await _workflowSmsService.SendOrderCompletedCustomerNotificationAsync(eventMessage.Order, eventMessage.Order.CustomerLanguageId);
			}
			if (prevOrderStatus != OrderStatus.Cancelled && os == OrderStatus.Cancelled)
			{
				await _workflowSmsService.SendOrderCancelledCustomerNotificationAsync(eventMessage.Order, eventMessage.Order.CustomerLanguageId);
			}
		}
	}

	public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
	{
		await _workflowSmsService.SendCustomerRegisteredNotificationMessageAsync(eventMessage.Customer, _localizationSettings.DefaultAdminLanguageId);
		switch (_customerSettings.UserRegistrationType)
		{
		case UserRegistrationType.EmailValidation:
		{
			IWorkflowSmsService workflowSmsService = _workflowSmsService;
			Customer customer = eventMessage.Customer;
			await workflowSmsService.SendCustomerEmailValidationMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);
			break;
		}
		case UserRegistrationType.Standard:
		{
			IWorkflowSmsService workflowSmsService = _workflowSmsService;
			Customer customer = eventMessage.Customer;
			await workflowSmsService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);
			break;
		}
		}
	}

	public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
	{
		foreach (Vendor item in await GetVendorsInOrderAsync(eventMessage.Order))
		{
			await _workflowSmsService.SendOrderPlacedVendorNotificationAsync(eventMessage.Order, item, _localizationSettings.DefaultAdminLanguageId);
		}
		await _workflowSmsService.SendOrderPlacedCustomerNotificationAsync(eventMessage.Order, _localizationSettings.DefaultAdminLanguageId);
		await _workflowSmsService.SendOrderPlacedAdminNotificationAsync(eventMessage.Order, _localizationSettings.DefaultAdminLanguageId);
	}

	public async Task HandleEventAsync(OrderPaidEvent eventMessage)
	{
		foreach (Vendor item in await GetVendorsInOrderAsync(eventMessage.Order))
		{
			await _workflowSmsService.SendOrderPaidVendorNotificationAsync(eventMessage.Order, item, _localizationSettings.DefaultAdminLanguageId);
		}
		await _workflowSmsService.SendOrderPaidCustomerNotificationAsync(eventMessage.Order, _localizationSettings.DefaultAdminLanguageId);
		await _workflowSmsService.SendOrderPaidAdminNotificationAsync(eventMessage.Order, _localizationSettings.DefaultAdminLanguageId);
	}

	public async Task HandleEventAsync(ShipmentSentEvent eventMessage)
	{
		IWorkflowSmsService workflowSmsService = _workflowSmsService;
		Shipment shipment = eventMessage.Shipment;
		await workflowSmsService.SendShipmentSentCustomerNotificationAsync(shipment, (await _orderService.GetOrderByIdAsync(eventMessage.Shipment.OrderId)).CustomerLanguageId);
	}

	public async Task HandleEventAsync(ShipmentDeliveredEvent eventMessage)
	{
		IWorkflowSmsService workflowSmsService = _workflowSmsService;
		Shipment shipment = eventMessage.Shipment;
		await workflowSmsService.SendShipmentDeliveredCustomerNotificationAsync(shipment, (await _orderService.GetOrderByIdAsync(eventMessage.Shipment.OrderId)).CustomerLanguageId);
	}

	public async Task HandleEventAsync(OrderRefundedEvent eventMessage)
	{
		await _workflowSmsService.SendOrderRefundedAdminNotificationAsync(eventMessage.Order, eventMessage.Amount, eventMessage.Order.CustomerLanguageId);
		await _workflowSmsService.SendOrderRefundedCustomerNotificationAsync(eventMessage.Order, eventMessage.Amount, eventMessage.Order.CustomerLanguageId);
	}

	public async Task HandleEventAsync(EntityInsertedEvent<ForumTopic> eventMessage)
	{
		IPagedList<ForumSubscription> subscriptions = await _forumService.GetAllSubscriptionsAsync(0, eventMessage.Entity.ForumId);
		int languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
		foreach (ForumSubscription subscription in subscriptions)
		{
			if (subscription.CustomerId != eventMessage.Entity.CustomerId && !string.IsNullOrEmpty((await _customerService.GetCustomerByIdAsync(subscription.CustomerId)).Email))
			{
				Forum forum = await _forumService.GetForumByIdAsync(eventMessage.Entity.ForumId);
				IWorkflowSmsService workflowSmsService = _workflowSmsService;
				await workflowSmsService.SendNewForumTopicMessageAsync(await _customerService.GetCustomerByIdAsync(subscription.CustomerId), eventMessage.Entity, forum, languageId);
			}
		}
	}

	public async Task HandleEventAsync(EntityInsertedEvent<ForumPost> eventMessage)
	{
		ForumTopic forumTopic = await _forumService.GetTopicByIdAsync(eventMessage.Entity.TopicId);
		Forum forum = await _forumService.GetForumByIdAsync(forumTopic.ForumId);
		IPagedList<ForumSubscription> subscriptions = await _forumService.GetAllSubscriptionsAsync(0, 0, forumTopic.Id);
		int languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
		int friendlyTopicPageIndex = await _forumService.CalculateTopicPageIndexAsync(eventMessage.Entity.TopicId, (_forumSettings.PostsPageSize > 0) ? _forumSettings.PostsPageSize : 10, eventMessage.Entity.Id) + 1;
		foreach (ForumSubscription subscription in subscriptions)
		{
			if (subscription.CustomerId != eventMessage.Entity.CustomerId && !string.IsNullOrEmpty((await _customerService.GetCustomerByIdAsync(subscription.CustomerId)).Email))
			{
				IWorkflowSmsService workflowSmsService = _workflowSmsService;
				await workflowSmsService.SendNewForumPostMessageAsync(await _customerService.GetCustomerByIdAsync(subscription.CustomerId), eventMessage.Entity, forumTopic, forum, friendlyTopicPageIndex, languageId);
			}
		}
	}

	public async Task HandleEventAsync(EntityInsertedEvent<PrivateMessage> eventMessage)
	{
		IWorkflowSmsService workflowSmsService = _workflowSmsService;
		PrivateMessage entity = eventMessage.Entity;
		await workflowSmsService.SendPrivateMessageNotificationAsync(entity, (await _workContext.GetWorkingLanguageAsync()).Id);
	}

	public async Task HandleEventAsync(CustomerActivatedEvent eventMessage)
	{
		IWorkflowSmsService workflowSmsService = _workflowSmsService;
		Customer customer = eventMessage.Customer;
		await workflowSmsService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);
	}
}
