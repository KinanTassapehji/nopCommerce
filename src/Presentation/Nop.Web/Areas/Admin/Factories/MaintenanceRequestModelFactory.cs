using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Common;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories;

/// <summary>
/// Represents the maintenance request model factory implementation
/// </summary>
public partial class MaintenanceRequestModelFactory : IMaintenanceRequestModelFactory
{
    #region Fields

    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly ILocalizationService _localizationService;
    protected readonly IMaintenanceRequestService _maintenanceRequestService;

    #endregion

    #region Ctor

    public MaintenanceRequestModelFactory(IDateTimeHelper dateTimeHelper,
        ILocalizationService localizationService,
        IMaintenanceRequestService maintenanceRequestService)
    {
        _dateTimeHelper = dateTimeHelper;
        _localizationService = localizationService;
        _maintenanceRequestService = maintenanceRequestService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Prepare available statuses
    /// </summary>
    /// <param name="items">Status items</param>
    /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task PrepareStatusesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem)
    {
        foreach (var statusItem in await MaintenanceRequestStatus.New.ToSelectListAsync(false))
            items.Add(statusItem);

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });
    }

    /// <summary>
    /// Prepare a filter list out of the values a column has actually been filled with
    /// </summary>
    /// <param name="items">Filter items</param>
    /// <param name="selector">Column to collect</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task PrepareUsedValuesAsync(IList<SelectListItem> items,
        Func<IQueryable<MaintenanceRequest>, IQueryable<string>> selector)
    {
        items.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = string.Empty });

        foreach (var value in await _maintenanceRequestService.GetUsedValuesAsync(selector))
            items.Add(new SelectListItem { Text = value, Value = value });
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare maintenance request search model
    /// </summary>
    /// <param name="searchModel">Maintenance request search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the maintenance request search model
    /// </returns>
    public virtual async Task<MaintenanceRequestSearchModel> PrepareMaintenanceRequestSearchModelAsync(MaintenanceRequestSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare available statuses
        await PrepareStatusesAsync(searchModel.AvailableStatuses, true);

        //prepare available warranty options
        searchModel.AvailableWarrantyOptions.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });
        searchModel.AvailableWarrantyOptions.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Yes"), Value = "1" });
        searchModel.AvailableWarrantyOptions.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.No"), Value = "2" });

        //prepare the brands that have actually been sent in
        await PrepareUsedValuesAsync(searchModel.AvailableBrands, query => query.Select(request => request.Brand));

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged maintenance request list model
    /// </summary>
    /// <param name="searchModel">Maintenance request search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the maintenance request list model
    /// </returns>
    public virtual async Task<MaintenanceRequestListModel> PrepareMaintenanceRequestListModelAsync(MaintenanceRequestSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get parameters to filter requests
        var startDateValue = !searchModel.SearchStartDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchStartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
        var endDateValue = !searchModel.SearchEndDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchEndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        //get maintenance requests
        var maintenanceRequests = await _maintenanceRequestService.SearchMaintenanceRequestsAsync(
            createdFromUtc: startDateValue,
            createdToUtc: endDateValue,
            statusId: searchModel.SearchStatusId > 0 ? searchModel.SearchStatusId : null,
            inWarranty: searchModel.SearchInWarrantyId == 0 ? null : searchModel.SearchInWarrantyId == 1,
            fullName: searchModel.SearchFullName,
            phoneNumber: searchModel.SearchPhoneNumber,
            email: searchModel.SearchEmail,
            city: searchModel.SearchCity,
            area: searchModel.SearchArea,
            brand: searchModel.SearchBrand,
            deviceType: searchModel.SearchDeviceType,
            modelNumber: searchModel.SearchModelNumber,
            problem: searchModel.SearchProblem,
            adminComment: searchModel.SearchAdminComment,
            keywords: searchModel.SearchKeywords,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new MaintenanceRequestListModel().PrepareToGridAsync(searchModel, maintenanceRequests, () =>
        {
            return maintenanceRequests.SelectAwait(async maintenanceRequest =>
            {
                //fill in model values from the entity
                var maintenanceRequestModel = maintenanceRequest.ToModel<MaintenanceRequestModel>();

                //convert dates to the user time
                maintenanceRequestModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(maintenanceRequest.CreatedOnUtc, DateTimeKind.Utc);

                //fill in additional values (not existing in the entity)
                maintenanceRequestModel.StatusName = await _localizationService.GetLocalizedEnumAsync(maintenanceRequest.Status);

                return maintenanceRequestModel;
            });
        });

        return model;
    }

    /// <summary>
    /// Prepare maintenance request model
    /// </summary>
    /// <param name="model">Maintenance request model</param>
    /// <param name="maintenanceRequest">Maintenance request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the maintenance request model
    /// </returns>
    public virtual async Task<MaintenanceRequestModel> PrepareMaintenanceRequestModelAsync(MaintenanceRequestModel model,
        MaintenanceRequest maintenanceRequest)
    {
        ArgumentNullException.ThrowIfNull(maintenanceRequest);

        if (model == null)
        {
            model = maintenanceRequest.ToModel<MaintenanceRequestModel>();
            model.StatusName = await _localizationService.GetLocalizedEnumAsync(maintenanceRequest.Status);
        }

        model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(maintenanceRequest.CreatedOnUtc, DateTimeKind.Utc);

        await PrepareStatusesAsync(model.AvailableStatuses, false);

        return model;
    }

    #endregion
}