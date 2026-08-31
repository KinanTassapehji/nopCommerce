using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Tax;
using NopStation.Plugin.Widgets.ProductRibbon.Infrastructure.Cache;
using NopStation.Plugin.Widgets.ProductRibbon.Models;
using NopStation.Plugin.Widgets.ProductRibbon.Services;

namespace NopStation.Plugin.Widgets.ProductRibbon.Factories;

public class ProductRibbonModelFactory : IProductRibbonModelFactory
{
	private readonly ITaxService _taxService;

	private readonly IPriceCalculationService _priceCalculationService;

	private readonly ProductRibbonSettings _productRibbonSettings;

	private readonly IStaticCacheManager _staticCacheManager;

	private readonly IPermissionService _permissionService;

	private readonly IWorkContext _workContext;

	private readonly IStoreContext _storeContext;

	private readonly ILocalizationService _localizationService;

	private readonly IProductService _productService;

	private readonly IBestSellerService _bestSellerService;

	public ProductRibbonModelFactory(ITaxService taxService, IPriceCalculationService priceCalculationService, ProductRibbonSettings productRibbonSettings, IStaticCacheManager staticCacheManger, IPermissionService permissionService, IWorkContext workContext, IStoreContext storeContext, ILocalizationService localizationService, IProductService productService, IBestSellerService bestSellerService)
	{
		_taxService = taxService;
		_priceCalculationService = priceCalculationService;
		_productRibbonSettings = productRibbonSettings;
		_staticCacheManager = staticCacheManger;
		_permissionService = permissionService;
		_workContext = workContext;
		_storeContext = storeContext;
		_localizationService = localizationService;
		_productService = productService;
		_bestSellerService = bestSellerService;
	}

	public async Task<ProductRibbonModel> PrepareProductRibbonModelAsync(Product product)
	{
		IStaticCacheManager staticCacheManager = _staticCacheManager;
		CacheKey pRODUCT_RIBBON_MODEL_KEY = ModelCacheEventConsumer.PRODUCT_RIBBON_MODEL_KEY;
		object obj = product.Id;
		object obj2 = (await _workContext.GetCurrentCustomerAsync()).Id;
		Language language = await _workContext.GetWorkingLanguageAsync();
		CacheKey key = staticCacheManager.PrepareKeyForDefaultCache(pRODUCT_RIBBON_MODEL_KEY, obj, obj2, language.Id);
		return await _staticCacheManager.GetAsync(key, async delegate
		{
			ProductRibbonModel model = new ProductRibbonModel();
			if (_productRibbonSettings.EnableBestSellerRibbon)
			{
				BestsellersReportLine bestsellersReportLine = await _bestSellerService.BestSellerReportAsync(product.Id);
				model.IsBestSeller = bestsellersReportLine != null && bestsellersReportLine.TotalAmount > _productRibbonSettings.MinimumAmountSold && bestsellersReportLine.TotalQuantity > _productRibbonSettings.MinimumQuantitySold;
			}
			if (_productRibbonSettings.EnableNewRibbon)
			{
				model.IsNew = product.MarkAsNew && (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) && (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow);
			}
			bool flag = _productRibbonSettings.EnableDiscountRibbon && product.ProductType == ProductType.SimpleProduct;
			if (flag)
			{
				flag = await _permissionService.AuthorizeAsync("PublicStore.DisplayPrices");
			}
			if (flag && !product.CustomerEntersPrice && !product.CallForPrice)
			{
				ITaxService taxService = _taxService;
				Product product2 = product;
				IPriceCalculationService priceCalculationService = _priceCalculationService;
				Product product3 = product;
				decimal price = (await taxService.GetProductPriceAsync(product2, (await priceCalculationService.GetFinalPriceAsync(product3, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync())).Item2)).Item1;
				TierPrice tierPrice = (await _productService.GetTierPricesByProductAsync(product.Id)).FirstOrDefault((TierPrice x) => x.Quantity == 0 && !x.CustomerRoleId.HasValue);
				decimal num = ((product.OldPrice > 0m) ? product.OldPrice : product.Price);
				decimal num2 = tierPrice?.Price ?? price;
				if (num > 0m)
				{
					decimal num3 = num - num2;
					if (num3 > 0m)
					{
						int productPrice = (int)Math.Ceiling(num3 * 100m / num);
						ProductRibbonModel productRibbonModel = model;
						productRibbonModel.Discount = string.Format(await _localizationService.GetResourceAsync("NopStation.ProductRibbon.RibbonText.Discount"), productPrice);
					}
				}
			}
			return model;
		});
	}
}
