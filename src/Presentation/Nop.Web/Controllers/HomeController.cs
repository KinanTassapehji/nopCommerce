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
    /// <param name="excludedIds">Comma-separated ids of products the page already shows above the feed</param>
    public virtual async Task<IActionResult> Products(int pageNumber = 1, string excludedIds = null)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var pageSize = _catalogSettings.DefaultCategoryPageSize;
        pageNumber = Math.Max(pageNumber, 1);

        //the bands above the feed print product boxes of their own, and the feed leaves those out.
        //Dropping them from a page that was already cut left short pages, so they come out first
        //and the page is filled to pageSize from what is left. The caller sends the list because
        //only it knows what the widget carousels rendered.
        //ponytail: reads pageNumber pages to serve one - an excluded product can sit anywhere in
        //the order, so there is no offset to skip straight to. Fine for a feed read from the top;
        //a keyset cursor is the fix if the pager ever runs deep.
        var excluded = (excludedIds ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id, out var productId) ? productId : 0)
            .Where(id => id > 0)
            .ToList();

        var products = await _productService.SearchProductsAsync(
            pageSize: pageNumber * pageSize + excluded.Count,
            storeId: store.Id,
            visibleIndividuallyOnly: true);

        var page = products.Where(product => !excluded.Contains(product.Id))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        //no more products, the caller stops requesting
        if (page.Count == 0)
            return Content(string.Empty);

        var pagedProducts = new PagedList<Product>(page, pageNumber - 1, pageSize,
            Math.Max(products.TotalCount - excluded.Count, page.Count));

        var model = new CatalogProductsModel
        {
            Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(pagedProducts)).ToList()
        };
        model.LoadPagedList(pagedProducts);

        return PartialView("_Products", model);
    }

    #endregion
}