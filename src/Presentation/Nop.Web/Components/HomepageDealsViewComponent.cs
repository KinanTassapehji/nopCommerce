using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components;

/// <summary>
/// The home page "deals" band: products whose old price is above the selling price
/// </summary>
public partial class HomepageDealsViewComponent : NopViewComponent
{
    protected readonly IProductModelFactory _productModelFactory;
    protected readonly IProductService _productService;
    protected readonly IStoreMappingService _storeMappingService;

    public HomepageDealsViewComponent(IProductModelFactory productModelFactory,
        IProductService productService,
        IStoreMappingService storeMappingService)
    {
        _productModelFactory = productModelFactory;
        _productService = productService;
        _storeMappingService = storeMappingService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
    {
        //ponytail: ten, one catalogue page of them - a setting if merchandising ever asks
        var products = await (await _productService.GetDiscountedProductsAsync())
            //store mapping
            .WhereAwait(async p => await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            .Take(10)
            .ToListAsync();

        if (!products.Any())
            return Content("");

        var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();

        return View(model);
    }
}
