using Microsoft.AspNetCore.Mvc;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers;

public partial class MaintenanceRequestController : BaseAdminController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly IMaintenanceRequestModelFactory _maintenanceRequestModelFactory;
    protected readonly IMaintenanceRequestService _maintenanceRequestService;
    protected readonly INotificationService _notificationService;

    #endregion

    #region Ctor

    public MaintenanceRequestController(ILocalizationService localizationService,
        IMaintenanceRequestModelFactory maintenanceRequestModelFactory,
        IMaintenanceRequestService maintenanceRequestService,
        INotificationService notificationService)
    {
        _localizationService = localizationService;
        _maintenanceRequestModelFactory = maintenanceRequestModelFactory;
        _maintenanceRequestService = maintenanceRequestService;
        _notificationService = notificationService;
    }

    #endregion

    #region Methods

    public virtual IActionResult Index()
    {
        return RedirectToAction("List");
    }

    [CheckPermission(StandardPermission.Orders.MAINTENANCE_REQUESTS_VIEW)]
    public virtual async Task<IActionResult> List()
    {
        //prepare model
        var model = await _maintenanceRequestModelFactory
            .PrepareMaintenanceRequestSearchModelAsync(new MaintenanceRequestSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Orders.MAINTENANCE_REQUESTS_VIEW)]
    public virtual async Task<IActionResult> MaintenanceRequestList(MaintenanceRequestSearchModel searchModel)
    {
        //prepare model
        var model = await _maintenanceRequestModelFactory.PrepareMaintenanceRequestListModelAsync(searchModel);

        return Json(model);
    }

    [CheckPermission(StandardPermission.Orders.MAINTENANCE_REQUESTS_VIEW)]
    public virtual async Task<IActionResult> Edit(int id)
    {
        //try to get a maintenance request with the specified id
        var maintenanceRequest = await _maintenanceRequestService.GetMaintenanceRequestByIdAsync(id);
        if (maintenanceRequest == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _maintenanceRequestModelFactory.PrepareMaintenanceRequestModelAsync(null, maintenanceRequest);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Orders.MAINTENANCE_REQUESTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Edit(MaintenanceRequestModel model, bool continueEditing)
    {
        //try to get a maintenance request with the specified id
        var maintenanceRequest = await _maintenanceRequestService.GetMaintenanceRequestByIdAsync(model.Id);
        if (maintenanceRequest == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            //the customer's own words are left alone; only the staff fields are editable
            maintenanceRequest.StatusId = model.StatusId;
            maintenanceRequest.AdminComment = model.AdminComment;
            await _maintenanceRequestService.UpdateMaintenanceRequestAsync(maintenanceRequest);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Sales.MaintenanceRequests.Updated"));

            return continueEditing ? RedirectToAction("Edit", new { id = maintenanceRequest.Id }) : RedirectToAction("List");
        }

        //prepare model
        model = await _maintenanceRequestModelFactory.PrepareMaintenanceRequestModelAsync(model, maintenanceRequest);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Orders.MAINTENANCE_REQUESTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        //try to get a maintenance request with the specified id
        var maintenanceRequest = await _maintenanceRequestService.GetMaintenanceRequestByIdAsync(id);
        if (maintenanceRequest == null)
            return RedirectToAction("List");

        await _maintenanceRequestService.DeleteMaintenanceRequestAsync(maintenanceRequest);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Sales.MaintenanceRequests.Deleted"));

        return RedirectToAction("List");
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Orders.MAINTENANCE_REQUESTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (selectedIds == null || !selectedIds.Any())
            return NoContent();

        await _maintenanceRequestService.DeleteMaintenanceRequestsAsync(
            await _maintenanceRequestService.GetMaintenanceRequestsByIdsAsync(selectedIds.ToArray()));

        return Json(new { Result = true });
    }

    #endregion
}