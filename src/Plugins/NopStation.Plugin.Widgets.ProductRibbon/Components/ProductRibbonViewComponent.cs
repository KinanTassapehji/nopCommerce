using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.ProductRibbon.Factories;

namespace NopStation.Plugin.Widgets.ProductRibbon.Components;

public class ProductRibbonViewComponent : NopStationViewComponent
{
	private readonly IProductService _productService;

	private readonly IProductRibbonModelFactory _productRibbonModelFactory;

	public ProductRibbonViewComponent(IProductService productService, IProductRibbonModelFactory productRibbonModelFactory)
	{
		_productService = productService;
		_productRibbonModelFactory = productRibbonModelFactory;
	}

	public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
	{
		int num = 0;
		if (additionalData.GetType() == typeof(ProductDetailsModel))
		{
			num = (additionalData as ProductDetailsModel).Id;
		}
		else if (additionalData.GetType() == typeof(ProductOverviewModel))
		{
			num = (additionalData as ProductOverviewModel).Id;
		}
		else if (additionalData.GetType() == typeof(int))
		{
			num = Convert.ToInt32(additionalData);
		}
		if (num == 0)
		{
			return Content("");
		}
		Product product = await _productService.GetProductByIdAsync(num);
		if (product == null)
		{
			return Content("");
		}
		return View(await _productRibbonModelFactory.PrepareProductRibbonModelAsync(product));
	}
}
