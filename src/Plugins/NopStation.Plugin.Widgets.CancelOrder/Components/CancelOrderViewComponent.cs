using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Web.Models.Order;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.CancelOrder.Models;

namespace NopStation.Plugin.Widgets.CancelOrder.Components;

public class CancelOrderViewComponent : NopStationViewComponent
{
	private readonly IOrderService _orderService;

	private readonly IWorkContext _workContext;

	private readonly CancelOrderSettings _cancelOrderSettings;

	public CancelOrderViewComponent(IOrderService orderService, IWorkContext workContext, CancelOrderSettings cancelOrderSettings)
	{
		_orderService = orderService;
		_workContext = workContext;
		_cancelOrderSettings = cancelOrderSettings;
	}

	public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
	{
		int orderId;
		if (additionalData.GetType() == typeof(OrderDetailsModel))
		{
			OrderDetailsModel orderDetailsModel = additionalData as OrderDetailsModel;
			orderId = orderDetailsModel.Id;
		}
		else if (!int.TryParse(additionalData.ToString(), out orderId))
		{
			return Content("");
		}
		Order order = await _orderService.GetOrderByIdAsync(orderId);
		if (order == null || order.Deleted)
		{
			return Content("");
		}
		int customerId = order.CustomerId;
		if (customerId != (await _workContext.GetCurrentCustomerAsync()).Id)
		{
			return Content("");
		}
		if (!_cancelOrderSettings.CancellableOrderStatuses.Contains(order.OrderStatusId) || !_cancelOrderSettings.CancellablePaymentStatuses.Contains(order.PaymentStatusId) || !_cancelOrderSettings.CancellableShippingStatuses.Contains(order.ShippingStatusId))
		{
			return Content("");
		}
		PublicInfoModel model = new PublicInfoModel
		{
			OrderId = orderId
		};
		return View(model);
	}
}
