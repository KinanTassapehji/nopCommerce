using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Widgets.QuickView.Models;

namespace NopStation.Plugin.Widgets.QuickView.Controllers;

public class QuickViewController : BasePluginController
{
	private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;

	private readonly ICustomerActivityService _customerActivityService;

	private readonly IProductModelFactory _productModelFactory;

	private readonly IStoreMappingService _storeMappingService;

	private readonly ILocalizationService _localizationService;

	private readonly IPermissionService _permissionService;

	private readonly QuickViewSettings _quickViewSettings;

	private readonly CatalogSettings _catalogSettings;

	private readonly IProductService _productService;

	private readonly IAclService _aclService;

	private readonly IWorkContext _workContext;

	private readonly IStoreContext _storeContext;

	private readonly IPluginService _pluginService;

	private readonly IWidgetPluginManager _widgetPluginManager;

	private readonly ILogger _logger;

	public QuickViewController(IRecentlyViewedProductsService recentlyViewedProductsService, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IProductModelFactory productModelFactory, IStoreMappingService storeMappingService, IPermissionService permissionService, QuickViewSettings quickViewSettings, IProductService productService, CatalogSettings catalogSettings, IAclService aclService, IWorkContext workContext, IStoreContext storeContext, IPluginService pluginService, IWidgetPluginManager widgetPluginManager, ILogger logger)
	{
		_recentlyViewedProductsService = recentlyViewedProductsService;
		_customerActivityService = customerActivityService;
		_localizationService = localizationService;
		_storeMappingService = storeMappingService;
		_productModelFactory = productModelFactory;
		_permissionService = permissionService;
		_quickViewSettings = quickViewSettings;
		_productService = productService;
		_catalogSettings = catalogSettings;
		_aclService = aclService;
		_workContext = workContext;
		_storeContext = storeContext;
		_pluginService = pluginService;
		_widgetPluginManager = widgetPluginManager;
		_logger = logger;
	}

	[HttpGet]
	public async Task<IActionResult> ProductDetails(int productId, int updatecartitemid = 0)
	{
		Product product = await _productService.GetProductByIdAsync(productId);
		if (product == null || product.Deleted)
		{
			return NotFound();
		}
		bool flag2 = !product.Published && !_catalogSettings.AllowViewUnpublishedProductPage;
		if (!flag2)
		{
			flag2 = !(await _storeMappingService.AuthorizeAsync(product));
		}
		bool notAvailable = flag2 || !_productService.ProductIsAvailable(product);
		flag2 = await _permissionService.AuthorizeAsync("Security.AccessAdminPanel");
		if (flag2)
		{
			flag2 = await _permissionService.AuthorizeAsync("Catalog.ProductsCreateEditDelete");
		}
		bool flag3 = flag2;
		if (notAvailable && !flag3)
		{
			return NotFound();
		}
		if (!product.VisibleIndividually)
		{
			Product product2 = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
			if (product2 != null)
			{
				return NotFound();
			}
			product = product2;
		}
		QuickViewProductDetailsModel quickViewProductDetailsModel = new QuickViewProductDetailsModel();
		QuickViewProductDetailsModel quickViewProductDetailsModel2 = quickViewProductDetailsModel;
		quickViewProductDetailsModel2.ProductDetailsModel = await _productModelFactory.PrepareProductDetailsModelAsync(product);
		quickViewProductDetailsModel.ShowAlsoPurchasedProducts = _quickViewSettings.ShowAlsoPurchasedProducts;
		quickViewProductDetailsModel.ShowRelatedProducts = _quickViewSettings.ShowRelatedProducts;
		quickViewProductDetailsModel.ShowAddToWishlistButton = _quickViewSettings.ShowAddToWishlistButton;
		quickViewProductDetailsModel.ShowAvailability = _quickViewSettings.ShowAvailability;
		quickViewProductDetailsModel.ShowProductEmailAFriendButton = _quickViewSettings.ShowProductEmailAFriendButton;
		quickViewProductDetailsModel.Id = product.Id;
		quickViewProductDetailsModel.ShowCompareProductsButton = _quickViewSettings.ShowCompareProductsButton;
		quickViewProductDetailsModel.ShowDeliveryInfo = _quickViewSettings.ShowDeliveryInfo;
		quickViewProductDetailsModel.ShowFullDescription = _quickViewSettings.ShowFullDescription;
		quickViewProductDetailsModel.ShowProductManufacturers = _quickViewSettings.ShowProductManufacturers;
		quickViewProductDetailsModel.ShowProductReviewOverview = _quickViewSettings.ShowProductReviewOverview;
		quickViewProductDetailsModel.ShowShortDescription = _quickViewSettings.ShowShortDescription;
		quickViewProductDetailsModel.ShowProductTags = _quickViewSettings.ShowProductTags;
		QuickViewProductDetailsModel model = quickViewProductDetailsModel;
		if (model.ProductDetailsModel.CustomProperties.ContainsKey("AjaxLoad"))
		{
			model.ProductDetailsModel.CustomProperties.Remove("AjaxLoad");
		}
		model.ProductDetailsModel.CustomProperties["AjaxLoad"] = "true";
		string productTemplateViewPath = await _productModelFactory.PrepareProductTemplateViewPathAsync(product);
		await _recentlyViewedProductsService.AddProductToRecentlyViewedListAsync(product.Id);
		ICustomerActivityService customerActivityService = _customerActivityService;
		await customerActivityService.InsertActivityAsync("PublicStore.ViewProduct", await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"), product);
		try
		{
			PluginDescriptor result = _pluginService.GetPluginDescriptorBySystemNameAsync<IWidgetPlugin>("NopStation.PictureZoom", LoadPluginsMode.InstalledOnly, _workContext.GetCurrentCustomerAsync().Result, _storeContext.GetCurrentStoreAsync().Result.Id).Result;
			if (result != null && _widgetPluginManager.IsPluginActive(result.Instance<IWidgetPlugin>()) && _quickViewSettings.EnablePictureZoom)
			{
				model.PictureZoomEnabled = true;
			}
			return Json(new
			{
				html = await RenderPartialViewToStringAsync("QuickView" + productTemplateViewPath, model)
			});
		}
		catch (Exception ex)
		{
			await _logger.ErrorAsync(ex.Message, ex);
			return Json(new
			{
				html = ""
			});
		}
	}
}
