using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Controllers;

public partial class HomeController : BasePublicController
{
    #region Fields

    protected readonly CatalogSettings _catalogSettings;
    protected readonly IProductModelFactory _productModelFactory;
    protected readonly IProductService _productService;
    protected readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public HomeController(CatalogSettings catalogSettings,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IStoreContext storeContext)
    {
        _catalogSettings = catalogSettings;
        _productModelFactory = productModelFactory;
        _productService = productService;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    [SaveLastContinueShoppingPage]
    public virtual IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Gets a page of products for the "all products" section of the home page (paged on desktop,
    /// infinitely scrolled on mobile - both ask for one page at a time)
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    public virtual async Task<IActionResult> Products(int pageNumber = 1)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var products = await _productService.SearchProductsAsync(pageIndex: Math.Max(pageNumber, 1) - 1,
            pageSize: _catalogSettings.DefaultCategoryPageSize,
            storeId: store.Id,
            visibleIndividuallyOnly: true);

        //no more products, the caller stops requesting
        if (!products.Any())
            return Content(string.Empty);

        var model = new CatalogProductsModel
        {
            Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList()
        };
        model.LoadPagedList(products);

        return PartialView("_Products", model);
    }

    #endregion
}