using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories;

/// <summary>
/// Represents the customer role model factory implementation
/// </summary>
public partial class CustomerRoleModelFactory : ICustomerRoleModelFactory
{
    #region Fields

    protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
    protected readonly ICustomerService _customerService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IPermissionService _permissionService;
    protected readonly IProductService _productService;
    protected readonly IUrlRecordService _urlRecordService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public CustomerRoleModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
        ICustomerService customerService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IProductService productService,
        IUrlRecordService urlRecordService,
        IWorkContext workContext)
    {
        _baseAdminModelFactory = baseAdminModelFactory;
        _customerService = customerService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _productService = productService;
        _urlRecordService = urlRecordService;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Permissions a role page never lists: the super administrators' own (they come with that role
    /// alone, so DefaultPermissionConfigManager is the record of them, whatever a role has been given
    /// since) and two no admin page shows - entering a closed store, and the HTML editor's pictures
    /// </summary>
    protected static readonly HashSet<string> HiddenPermissions = new DefaultPermissionConfigManager().AllConfigs
        .Where(config => config.DefaultCustomerRoles.Contains(NopCustomerDefaults.SuperAdministratorsRoleName))
        .Select(config => config.SystemName)
        .Concat([StandardPermission.PublicStore.ACCESS_CLOSED_STORE, StandardPermission.System.HTML_EDITOR_MANAGE_PICTURES])
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Whether a role page lists the permission; the save leaves the unlisted ones as they are
    /// </summary>
    public static bool IsListedPermission(string systemName) => !HiddenPermissions.Contains(systemName);

    /// <summary>
    /// The column a core permission goes in, from its system name ("Orders.OrdersView" is View);
    /// plugin permissions ("ManageNopStationQuickView") have no dot and never get a column
    /// </summary>
    protected virtual string GetPermissionColumn(string systemName)
    {
        if (!systemName.Contains('.'))
            return null;

        if (systemName.EndsWith("View", StringComparison.OrdinalIgnoreCase))
            return "View";
        if (systemName.Contains("CreateEdit", StringComparison.OrdinalIgnoreCase))
            return "Edit";
        if (systemName.EndsWith("ImportExport", StringComparison.OrdinalIgnoreCase))
            return "Import";

        return null;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare customer role search model
    /// </summary>
    /// <param name="searchModel">Customer role search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the customer role search model
    /// </returns>
    public virtual Task<CustomerRoleSearchModel> PrepareCustomerRoleSearchModelAsync(CustomerRoleSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare page parameters
        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    /// <summary>
    /// Prepare paged customer role list model
    /// </summary>
    /// <param name="searchModel">Customer role search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the customer role list model
    /// </returns>
    public virtual async Task<CustomerRoleListModel> PrepareCustomerRoleListModelAsync(CustomerRoleSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get customer roles
        //ponytail: unused system roles are hidden from this grid only; they still exist and work
        var hiddenRoles = new[] { NopCustomerDefaults.ForumModeratorsRoleName, NopCustomerDefaults.GuestsRoleName, NopCustomerDefaults.VendorsRoleName };
        var customerRoles = (await _customerService.GetAllCustomerRolesAsync(true))
            .Where(role => !hiddenRoles.Contains(role.SystemName)).ToList().ToPagedList(searchModel);

        //prepare grid model
        var model = await new CustomerRoleListModel().PrepareToGridAsync(searchModel, customerRoles, () =>
        {
            return customerRoles.SelectAwait(async role =>
            {
                //fill in model values from the entity
                var customerRoleModel = role.ToModel<CustomerRoleModel>();

                //fill in additional values (not existing in the entity)
                customerRoleModel.PurchasedWithProductName = (await _productService.GetProductByIdAsync(role.PurchasedWithProductId))?.Name;

                return customerRoleModel;
            });
        });

        return model;
    }

    /// <summary>
    /// Prepare customer role model
    /// </summary>
    /// <param name="model">Customer role model</param>
    /// <param name="customerRole">Customer role</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the customer role model
    /// </returns>
    public virtual async Task<CustomerRoleModel> PrepareCustomerRoleModelAsync(CustomerRoleModel model, CustomerRole customerRole, bool excludeProperties = false)
    {
        if (customerRole != null)
        {
            //fill in model values from the entity
            model ??= customerRole.ToModel<CustomerRoleModel>();
            model.PurchasedWithProductName = (await _productService.GetProductByIdAsync(customerRole.PurchasedWithProductId))?.Name;
        }

        //set default values for the new model
        if (customerRole == null)
            model.Active = true;

        //prepare available tax display types
        await _baseAdminModelFactory.PrepareTaxDisplayTypesAsync(model.TaxDisplayTypeValues, false);

        //system roles' permissions stay with super administrators (Access control list page)
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var isSuperAdmin = await _customerService.IsSuperAdminAsync(currentCustomer);
        model.CanEditPermissions = isSuperAdmin || customerRole?.IsSystemRole != true;
        if (!model.CanEditPermissions)
            return model;

        //only what the current user holds, so nobody can grant themselves more through a role
        var permissions = await (await _permissionService.GetAllPermissionRecordsAsync())
            .Where(permission => IsListedPermission(permission.SystemName))
            .WhereAwait(async permission => isSuperAdmin || await _permissionService.AuthorizeAsync(permission.SystemName, currentCustomer))
            .ToListAsync();

        if (!excludeProperties && customerRole != null)
        {
            model.SelectedPermissionIds = await permissions
                .WhereAwait(async permission => await _permissionService.AuthorizeAsync(permission.SystemName, customerRole.Id))
                .Select(permission => permission.Id)
                .ToListAsync();
        }

        //names read "Admin area. Orders. View": the first part repeats on almost every permission, so drop it
        //and show "Orders" as a row with "View" in one of its columns
        var names = new Dictionary<int, string[]>();
        foreach (var permission in permissions)
            names[permission.Id] = (await _localizationService.GetLocalizedPermissionNameAsync(permission))
                .Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var areaPrefixes = names.Values.Where(parts => parts.Length > 2).Select(parts => parts[0]).ToHashSet();

        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
        var categoryNames = new Dictionary<string, string>();
        foreach (var permission in permissions)
        {
            var parts = names[permission.Id];
            if (parts.Length > 1 && areaPrefixes.Contains(parts[0]))
                parts = parts[1..];
            if (parts.Length == 0)
                parts = [permission.Name];

            if (!categoryNames.TryGetValue(permission.Category, out var categoryName))
                categoryNames[permission.Category] = categoryName = await _localizationService.GetResourceAsync(
                    $"Admin.Customers.CustomerRoles.Permissions.Category.{permission.Category}",
                    languageId, false, CommonHelper.SplitCamelCaseWord(permission.Category));

            model.AvailablePermissions.Add(new CustomerRolePermissionModel
            {
                Id = permission.Id,
                Category = permission.Category,
                CategoryName = categoryName,
                Section = parts.Length == 1 ? null : string.Join(". ", parts[..^1]),
                Action = parts[^1],
                Column = GetPermissionColumn(permission.SystemName),
                Description = await _localizationService.GetResourceAsync(
                    $"Admin.Customers.CustomerRoles.Permissions.Description.{permission.SystemName}",
                    languageId, false, string.Empty, true),
                Selected = model.SelectedPermissionIds?.Contains(permission.Id) == true
            });
        }

        //security (it holds "Access admin area") first, then the admin menu's order; within a section View, Edit, Import, the rest
        string[] categoryOrder = ["Security", "Orders", "Catalog", "Customers", "Promotions", "ContentManagement",
            "Reports", "Configuration", "System", "PublicStore"];
        string[] columnOrder = ["View", "Edit", "Import"];
        model.AvailablePermissions = model.AvailablePermissions
            .OrderBy(permission => Array.IndexOf(categoryOrder, permission.Category) is var index and >= 0 ? index : categoryOrder.Length)
            .ThenBy(permission => permission.CategoryName)
            .ThenBy(permission => permission.Section)
            .ThenBy(permission => Array.IndexOf(columnOrder, permission.Column) is var index and >= 0 ? index : columnOrder.Length)
            .ToList();

        return model;
    }

    /// <summary>
    /// Prepare customer role product search model
    /// </summary>
    /// <param name="searchModel">Customer role product search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the customer role product search model
    /// </returns>
    public virtual async Task<CustomerRoleProductSearchModel> PrepareCustomerRoleProductSearchModelAsync(CustomerRoleProductSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //a vendor should have access only to his products
        searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

        //prepare available categories
        await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

        //prepare available manufacturers
        await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

        //prepare available stores
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        //prepare available vendors
        await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

        //prepare available product types
        await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

        //prepare page parameters
        searchModel.SetPopupGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged customer role product list model
    /// </summary>
    /// <param name="searchModel">Customer role product search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the customer role product list model
    /// </returns>
    public virtual async Task<CustomerRoleProductListModel> PrepareCustomerRoleProductListModelAsync(CustomerRoleProductSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
            searchModel.SearchVendorId = currentVendor.Id;

        //get products
        var products = await _productService.SearchProductsAsync(showHidden: true,
            categoryIds: new List<int> { searchModel.SearchCategoryId },
            manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new CustomerRoleProductListModel().PrepareToGridAsync(searchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                var productModel = product.ToModel<ProductModel>();

                productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                return productModel;
            });
        });

        return model;
    }

    #endregion
}