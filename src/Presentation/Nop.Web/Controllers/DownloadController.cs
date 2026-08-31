using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Http;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Controllers;

public partial class DownloadController : BasePublicController
{
    protected readonly IDownloadService _downloadService;
    protected readonly IOrderService _orderService;
    protected readonly IWorkContext _workContext;

    public DownloadController(IDownloadService downloadService,
        IOrderService orderService,
        IWorkContext workContext)
    {
        _downloadService = downloadService;
        _orderService = orderService;
        _workContext = workContext;
    }

    public virtual async Task<IActionResult> GetFileUpload(Guid downloadId)
    {
        var download = await _downloadService.GetDownloadByGuidAsync(downloadId);
        if (download == null)
            return Content("Download is not available any more.");

        //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
        //In this case, it is not relevant. Url may not be local.
        if (download.UseDownloadUrl)
            return new RedirectResult(download.DownloadUrl);

        //binary download
        if (download.DownloadBinary == null)
            return Content("Download data is not available any more.");

        //return result
        var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : downloadId.ToString();
        var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
        return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
    }

    //ignore SEO friendly URLs checks
    [CheckLanguageSeoCode(ignore: true)]
    public virtual async Task<IActionResult> GetOrderNoteFile(int orderNoteId)
    {
        var orderNote = await _orderService.GetOrderNoteByIdAsync(orderNoteId);
        if (orderNote == null)
            return InvokeHttp404();

        var order = await _orderService.GetOrderByIdAsync(orderNote.OrderId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || order.CustomerId != customer.Id)
            return Challenge();

        var download = await _downloadService.GetDownloadByIdAsync(orderNote.DownloadId);
        if (download == null)
            return Content("Download is not available any more.");

        //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
        //In this case, it is not relevant. Url may not be local.
        if (download.UseDownloadUrl)
            return new RedirectResult(download.DownloadUrl);

        //binary download
        if (download.DownloadBinary == null)
            return Content("Download data is not available any more.");

        //return result
        var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : orderNote.Id.ToString();
        var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
        return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
    }
}