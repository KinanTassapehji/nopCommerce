using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Http;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Controllers;

[AutoValidateAntiforgeryToken]
public partial class OrderController : BasePublicController
{
    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IOrderModelFactory _orderModelFactory;
    protected readonly IOrderProcessingService _orderProcessingService;
    protected readonly IOrderService _orderService;
    protected readonly IPaymentService _paymentService;
    protected readonly IPdfService _pdfService;
    protected readonly IShipmentService _shipmentService;
    protected readonly IWebHelper _webHelper;
    protected readonly IWorkContext _workContext;
    protected readonly OrderSettings _orderSettings;

    #endregion

    #region Ctor

    public OrderController(ICustomerService customerService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IOrderModelFactory orderModelFactory,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IPaymentService paymentService,
        IPdfService pdfService,
        IShipmentService shipmentService,
        IWebHelper webHelper,
        IWorkContext workContext,
        OrderSettings orderSettings)
    {
        _customerService = customerService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _orderModelFactory = orderModelFactory;
        _orderProcessingService = orderProcessingService;
        _orderService = orderService;
        _paymentService = paymentService;
        _pdfService = pdfService;
        _shipmentService = shipmentService;
        _webHelper = webHelper;
        _workContext = workContext;
        _orderSettings = orderSettings;
    }

    #endregion

    #region Methods

    //My account / Orders
    public virtual async Task<IActionResult> CustomerOrders(int? pageNumber, OrderHistoryPeriods limit)
    {
        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            return Challenge();

        var model = await _orderModelFactory.PrepareCustomerOrderListModelAsync(pageNumber, limit);
        return View(model);
    }

    //My account / Order details page
    public virtual async Task<IActionResult> Details(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return Challenge();

        var model = await _orderModelFactory.PrepareOrderDetailsModelAsync(order);
        return View(model);
    }

    //My account / Order details page / Print
    public virtual async Task<IActionResult> PrintOrderDetails(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return Challenge();

        var model = await _orderModelFactory.PrepareOrderDetailsModelAsync(order);
        model.PrintMode = true;

        return View("Details", model);
    }

    //My account / Order details page / PDF invoice
    [CheckLanguageSeoCode(ignore: true)]
    public virtual async Task<IActionResult> GetPdfInvoice(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return Challenge();

        byte[] bytes;
        await using (var stream = new MemoryStream())
        {
            await _pdfService.PrintOrderToPdfAsync(stream, order, await _workContext.GetWorkingLanguageAsync());
            bytes = stream.ToArray();
        }
        return File(bytes, MimeTypes.ApplicationPdf, string.Format(await _localizationService.GetResourceAsync("PDFInvoice.FileName"), order.CustomOrderNumber) + ".pdf");
    }

    public async Task<IActionResult> CancelOrder(int orderId)
    {
        if(!_orderSettings.AllowCustomersCancelOrders)
            return RedirectToRoute(NopRouteNames.Standard.ORDER_DETAILS, new { orderId });

        var order = await _orderService.GetOrderByIdAsync(orderId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (order == null || customer.Id != order.CustomerId)
            return Challenge();
        
        try
        {
            if (_orderProcessingService.CanCancelOrder(order))
                await _orderProcessingService.CancelOrderAsync(order, false);
        }
        catch
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Order.Cancel.Failed"));
            return RedirectToRoute(NopRouteNames.Standard.ORDER_DETAILS, new { orderId });
        }

        await _orderService.UpdateOrderAsync(order);
       _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Order.Cancelled"));

        return RedirectToRoute(NopRouteNames.Standard.ORDER_DETAILS, new { orderId });
    }

    //My account / Order details page / re-order
    public virtual async Task<IActionResult> ReOrder(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return Challenge();

        var warnings = await _orderProcessingService.ReOrderAsync(order);

        if (warnings.Any())
            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("ShoppingCart.ReorderWarning"));

        return RedirectToRoute(NopRouteNames.General.CART);
    }

    //My account / Order details page / Complete payment
    [HttpPost, ActionName("Details")]

    [FormValueRequired("repost-payment")]
    public virtual async Task<IActionResult> RePostPayment(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return Challenge();

        if (!await _paymentService.CanRePostProcessPaymentAsync(order))
            return RedirectToRoute(NopRouteNames.Standard.ORDER_DETAILS, new { orderId = orderId });

        var postProcessPaymentRequest = new PostProcessPaymentRequest
        {
            Order = order
        };
        await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

        if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
        {
            //redirection or POST has been done in PostProcessPayment
            return new EmptyResult();
        }

        //if no redirection has been done (to a third-party payment page)
        //theoretically it's not possible
        return RedirectToRoute(NopRouteNames.Standard.ORDER_DETAILS, new { orderId = orderId });
    }

    //My account / Order details page / Shipment details page
    public virtual async Task<IActionResult> ShipmentDetails(int shipmentId)
    {
        var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
        if (shipment == null)
            return Challenge();

        var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return Challenge();

        var model = await _orderModelFactory.PrepareShipmentDetailsModelAsync(shipment);
        return View(model);
    }

    #endregion
}