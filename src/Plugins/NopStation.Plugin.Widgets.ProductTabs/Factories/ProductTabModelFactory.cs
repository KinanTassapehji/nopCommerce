using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.ProductTabs.Domains;
using NopStation.Plugin.Widgets.ProductTabs.Helpers;
using NopStation.Plugin.Widgets.ProductTabs.Infrastructure.Cache;
using NopStation.Plugin.Widgets.ProductTabs.Models;
using NopStation.Plugin.Widgets.ProductTabs.Services;

namespace NopStation.Plugin.Widgets.ProductTabs.Factories;

public class ProductTabModelFactory : IProductTabModelFactory
{
	private readonly ICustomerService _customerService;

	private readonly IStaticCacheManager _staticCacheManager;

	private readonly ICategoryService _categoryService;

	private readonly IPictureService _pictureService;

	private readonly IProductService _productService;

	private readonly ILocalizationService _localizationService;

	private readonly IProductModelFactory _productModelFactory;

	private readonly IManufacturerService _manufacturerService;

	private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;

	private readonly IStoreContext _storeContext;

	private readonly IOrderReportService _orderReportService;

	private readonly IStoreMappingService _storeMappingService;

	private readonly IAclService _aclService;

	private readonly IProductTabService _productTabService;

	private readonly IWorkContext _workContext;

	public ProductTabModelFactory(ICustomerService customerService, IStaticCacheManager staticCacheManager, ICategoryService categoryService, IPictureService pictureService, IProductService productService, ILocalizationService localizationService, IProductModelFactory productModelFactory, IManufacturerService manufacturerService, IRecentlyViewedProductsService recentlyViewedProductsService, IStoreContext storeContext, IOrderReportService orderReportService, IStoreMappingService storeMappingService, IAclService aclService, IProductTabService productTabService, IWorkContext workContext)
	{
		_customerService = customerService;
		_staticCacheManager = staticCacheManager;
		_categoryService = categoryService;
		_pictureService = pictureService;
		_productService = productService;
		_localizationService = localizationService;
		_productModelFactory = productModelFactory;
		_manufacturerService = manufacturerService;
		_recentlyViewedProductsService = recentlyViewedProductsService;
		_storeContext = storeContext;
		_orderReportService = orderReportService;
		_storeMappingService = storeMappingService;
		_aclService = aclService;
		_productTabService = productTabService;
		_workContext = workContext;
	}

	protected async Task<PictureModel> PreparePictureModelAsync(ProductTab productTab)
	{
		PictureModel pictureModel = new PictureModel();
		PictureModel pictureModel2 = pictureModel;
		pictureModel2.ImageUrl = await _pictureService.GetPictureUrlAsync(productTab.PictureId);
		pictureModel.AlternateText = productTab.PictureAlt;
		pictureModel.Title = productTab.PictureTitle;
		return pictureModel;
	}

	public async Task<IList<ProductTabModel>> PrepareProductTabListModelAsync(List<ProductTab> productTabs)
	{
		ArgumentNullException.ThrowIfNull(productTabs, "productTabs");
		List<ProductTabModel> model = new List<ProductTabModel>();
		foreach (ProductTab productTab in productTabs)
		{
			List<ProductTabModel> list = model;
			list.Add(await PrepareProductTabModelAsync(productTab));
		}
		return model;
	}

	public async Task<IList<ProductTabModel>> PrepareProductTabListModelAsync(string widgetZone)
	{
		if (string.IsNullOrEmpty(widgetZone))
		{
			throw new ArgumentNullException("widgetZone");
		}
		if (!ProductTabHelper.TryGetWidgetZoneId(widgetZone, out var widgetZoneId))
		{
			return new List<ProductTabModel>();
		}
		IStaticCacheManager staticCacheManager = _staticCacheManager;
		CacheKey pRODUCT_TAB_MODEL_KEY = ModelCacheEventConsumer.PRODUCT_TAB_MODEL_KEY;
		object obj = widgetZoneId;
		ICustomerService customerService = _customerService;
		object obj2 = await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
		object obj3 = await _workContext.GetWorkingLanguageAsync();
		Store store = await _storeContext.GetCurrentStoreAsync();
		CacheKey key = staticCacheManager.PrepareKeyForDefaultCache(pRODUCT_TAB_MODEL_KEY, obj, obj2, obj3, store);
		return await _staticCacheManager.GetAsync(key, async delegate
		{
			IProductTabService productTabService = _productTabService;
			List<int> widgetZoneIds = new List<int> { widgetZoneId };
			List<ProductTab> productTabs = (await productTabService.GetAllProductTabsAsync(widgetZoneIds, hasItemsOnly: true, (await _storeContext.GetCurrentStoreAsync()).Id, true)).ToList();
			return await PrepareProductTabListModelAsync(productTabs);
		});
	}

	public async Task<ProductTabModel> PrepareProductTabModelAsync(ProductTab productTab)
	{
		ArgumentNullException.ThrowIfNull(productTab, "productTab");
		ProductTabModel productTabModel = new ProductTabModel
		{
			Id = productTab.Id,
			AutoPlay = productTab.AutoPlay
		};
		ProductTabModel productTabModel2 = productTabModel;
		productTabModel2.RTL = (await _workContext.GetWorkingLanguageAsync()).Rtl;
		productTabModel.CustomCssClass = productTab.CustomCssClass;
		productTabModel.AutoPlayHoverPause = productTab.AutoPlayHoverPause;
		productTabModel.AutoPlayTimeout = productTab.AutoPlayTimeout;
		productTabModel.Center = productTab.Center;
		productTabModel.LazyLoad = productTab.LazyLoad;
		productTabModel.LazyLoadEager = productTab.LazyLoadEager;
		productTabModel.Loop = productTab.Loop;
		productTabModel.Margin = productTab.Margin;
		productTabModel.Nav = productTab.Nav;
		productTabModel.StartPosition = productTab.StartPosition;
		productTabModel.CustomUrl = productTab.CustomUrl;
		ProductTabModel model = productTabModel;
		if (productTab.DisplayTitle)
		{
			model.DisplayTitle = productTab.DisplayTitle;
			productTabModel = model;
			productTabModel.Title = await _localizationService.GetLocalizedAsync(productTab, (ProductTab x) => x.TabTitle);
		}
		productTabModel = model;
		productTabModel.Picture = await PreparePictureModelAsync(productTab);
		IStaticCacheManager staticCacheManager = _staticCacheManager;
		CacheKey pRODUCT_TAB_ITEM_MODEL_KEY = ModelCacheEventConsumer.PRODUCT_TAB_ITEM_MODEL_KEY;
		object obj = productTab;
		ICustomerService customerService = _customerService;
		object obj2 = await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
		object obj3 = await _workContext.GetWorkingLanguageAsync();
		Store store = await _storeContext.GetCurrentStoreAsync();
		CacheKey key = staticCacheManager.PrepareKeyForDefaultCache(pRODUCT_TAB_ITEM_MODEL_KEY, obj, obj2, obj3, store);
		productTabModel = model;
		productTabModel.Items = await _staticCacheManager.GetAsync(key, async delegate
		{
			List<ProductTabItemModel> productTabItemModels = new List<ProductTabItemModel>();
			List<ProductTabItem> productTabItemsByProductTabId = _productTabService.GetProductTabItemsByProductTabId(productTab.Id);
			foreach (ProductTabItem item in productTabItemsByProductTabId)
			{
				List<ProductTabItemModel> list = productTabItemModels;
				list.Add(await PrepareProductTabItemModelAsync(item));
			}
			return productTabItemModels;
		});
		return model;
	}

	private async Task<ProductTabItemModel> PrepareProductTabItemModelAsync(ProductTabItem item)
	{
		ProductTabItemModel productTabItemModel = new ProductTabItemModel();
		ProductTabItemModel productTabItemModel2 = productTabItemModel;
		productTabItemModel2.Name = await _localizationService.GetLocalizedAsync(item, (ProductTabItem x) => x.Name);
		productTabItemModel.Id = item.Id;
		ProductTabItemModel model = productTabItemModel;
		IStaticCacheManager staticCacheManager = _staticCacheManager;
		CacheKey pRODUCT_TAB_ITEM_PRODUCT_MODEL_KEY = ModelCacheEventConsumer.PRODUCT_TAB_ITEM_PRODUCT_MODEL_KEY;
		ICustomerService customerService = _customerService;
		object obj = await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
		object obj2 = await _workContext.GetCurrentVendorAsync();
		Store store = await _storeContext.GetCurrentStoreAsync();
		staticCacheManager.PrepareKeyForDefaultCache(pRODUCT_TAB_ITEM_PRODUCT_MODEL_KEY, item, obj, obj2, store);
		int[] productIds = (from x in _productTabService.GetProductTabItemProductsByProductTabItemId(item.Id)
			select x.ProductId).ToArray();
		List<Product> source = (await _productService.GetProductsByIdsAsync(productIds)).Where((Product p) => p.Published).ToList();
		source = await source.WhereAwait(async delegate(Product p)
		{
			bool flag = await _storeMappingService.AuthorizeAsync(p);
			return flag && _productService.ProductIsAvailable(p);
		}).ToListAsync();
		productTabItemModel = model;
		productTabItemModel.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(source)).ToList();
		return model;
	}
}
