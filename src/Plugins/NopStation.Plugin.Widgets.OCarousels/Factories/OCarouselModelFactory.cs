using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.OCarousels.Domains;
using NopStation.Plugin.Widgets.OCarousels.Infrastructure.Cache;
using NopStation.Plugin.Widgets.OCarousels.Models;
using NopStation.Plugin.Widgets.OCarousels.Services;

namespace NopStation.Plugin.Widgets.OCarousels.Factories;

public class OCarouselModelFactory : IOCarouselModelFactory
{
	private readonly ICustomerService _customerService;

	private readonly ICategoryService _categoryService;

	private readonly IPictureService _pictureService;

	private readonly IProductService _productService;

	private readonly IUrlRecordService _urlRecordService;

	private readonly ILocalizationService _localizationService;

	private readonly MediaSettings _mediaSettings;

	private readonly IStaticCacheManager _cacheManager;

	private readonly IProductModelFactory _productModelFactory;

	private readonly IManufacturerService _manufacturerService;

	private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;

	private readonly IStoreContext _storeContext;

	private readonly IOrderReportService _orderReportService;

	private readonly IStoreMappingService _storeMappingService;

	private readonly IAclService _aclService;

	private readonly IOCarouselService _carouselService;

	private readonly IWorkContext _workContext;

	private readonly IStaticCacheManager _cacheKeyService;

	public OCarouselModelFactory(ICustomerService customerService, ICategoryService categoryService, IPictureService pictureService, IProductService productService, IUrlRecordService urlRecordService, ILocalizationService localizationService, MediaSettings mediaSettings, IStaticCacheManager staticCacheManager, IProductModelFactory productModelFactory, IManufacturerService manufacturerService, IRecentlyViewedProductsService recentlyViewedProductsService, IStoreContext storeContext, IOrderReportService orderReportService, IStoreMappingService storeMappingService, IAclService aclService, IOCarouselService carouselService, IWorkContext workContext, IStaticCacheManager cacheKeyService)
	{
		_customerService = customerService;
		_categoryService = categoryService;
		_pictureService = pictureService;
		_productService = productService;
		_urlRecordService = urlRecordService;
		_localizationService = localizationService;
		_mediaSettings = mediaSettings;
		_cacheManager = staticCacheManager;
		_productModelFactory = productModelFactory;
		_manufacturerService = manufacturerService;
		_recentlyViewedProductsService = recentlyViewedProductsService;
		_storeContext = storeContext;
		_orderReportService = orderReportService;
		_storeMappingService = storeMappingService;
		_aclService = aclService;
		_carouselService = carouselService;
		_workContext = workContext;
		_cacheKeyService = cacheKeyService;
	}

	protected async Task<IList<OCarouselModel.OCarouselManufacturerModel>> PrepareManufacturerListModel(OCarousel carousel)
	{
		IStaticCacheManager cacheKeyService = _cacheKeyService;
		CacheKey oCAROUSEL_MANUFACTURERS_MODEL_KEY = NopStation.Plugin.Widgets.OCarousels.Infrastructure.Cache.ModelCacheEventConsumer.OCAROUSEL_MANUFACTURERS_MODEL_KEY;
		object obj = carousel;
		ICustomerService customerService = _customerService;
		object obj2 = await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
		object obj3 = await _workContext.GetWorkingLanguageAsync();
		Store store = await _storeContext.GetCurrentStoreAsync();
		CacheKey key = cacheKeyService.PrepareKeyForDefaultCache(oCAROUSEL_MANUFACTURERS_MODEL_KEY, obj, obj2, obj3, store);
		return await _cacheManager.GetAsync(key, async delegate
		{
			List<Manufacturer> list = await (await (await _manufacturerService.GetAllManufacturersAsync()).ToListAsync()).WhereAwait(async (Manufacturer m) => await _storeMappingService.AuthorizeAsync(m)).Take(carousel.NumberOfItemsToShow).ToListAsync();
			List<OCarouselModel.OCarouselManufacturerModel> listModel = new List<OCarouselModel.OCarouselManufacturerModel>();
			foreach (Manufacturer manufacturer in list)
			{
				Picture picture = await _pictureService.GetPictureByIdAsync(manufacturer.PictureId);
				OCarouselModel.OCarouselManufacturerModel oCarouselManufacturerModel = new OCarouselModel.OCarouselManufacturerModel();
				OCarouselModel.OCarouselManufacturerModel oCarouselManufacturerModel2 = oCarouselManufacturerModel;
				oCarouselManufacturerModel2.Name = await _localizationService.GetLocalizedAsync(manufacturer, (Manufacturer x) => x.Name);
				OCarouselModel.OCarouselManufacturerModel oCarouselManufacturerModel3 = oCarouselManufacturerModel;
				oCarouselManufacturerModel3.SeName = await _urlRecordService.GetSeNameAsync(manufacturer);
				OCarouselModel.OCarouselManufacturerModel oCarouselManufacturerModel4 = oCarouselManufacturerModel;
				PictureModel pictureModel = new PictureModel();
				PictureModel pictureModel2 = pictureModel;
				pictureModel2.ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSize)).Item1;
				PictureModel pictureModel3 = pictureModel;
				pictureModel3.FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Item1;
				PictureModel pictureModel4 = pictureModel;
				string title = ((picture == null || string.IsNullOrEmpty(picture.TitleAttribute)) ? string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), manufacturer.Name) : picture.TitleAttribute);
				pictureModel4.Title = title;
				PictureModel pictureModel5 = pictureModel;
				string alternateText = ((picture == null || string.IsNullOrEmpty(picture.AltAttribute)) ? string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), manufacturer.Name) : picture.AltAttribute);
				pictureModel5.AlternateText = alternateText;
				oCarouselManufacturerModel4.PictureModel = pictureModel;
				listModel.Add(oCarouselManufacturerModel);
			}
			return listModel;
		});
	}

	protected async Task<IList<OCarouselModel.OCarouselCategoryModel>> PrepareCategoryListModelAsync(OCarousel carousel)
	{
		IStaticCacheManager cacheKeyService = _cacheKeyService;
		CacheKey oCAROUSEL_CATEGORIES_MODEL_KEY = NopStation.Plugin.Widgets.OCarousels.Infrastructure.Cache.ModelCacheEventConsumer.OCAROUSEL_CATEGORIES_MODEL_KEY;
		object obj = carousel;
		ICustomerService customerService = _customerService;
		object obj2 = await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
		object obj3 = await _workContext.GetWorkingLanguageAsync();
		Store store = await _storeContext.GetCurrentStoreAsync();
		CacheKey key = cacheKeyService.PrepareKeyForDefaultCache(oCAROUSEL_CATEGORIES_MODEL_KEY, obj, obj2, obj3, store);
		return await _cacheManager.GetAsync(key, async delegate
		{
			IList<Category> list = await (await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync()).WhereAwait(async delegate(Category p)
			{
				bool flag = await _aclService.AuthorizeAsync(p);
				if (flag)
				{
					flag = await _storeMappingService.AuthorizeAsync(p);
				}
				return flag;
			}).Take(carousel.NumberOfItemsToShow).ToListAsync();
			List<OCarouselModel.OCarouselCategoryModel> listModel = new List<OCarouselModel.OCarouselCategoryModel>();
			foreach (Category category in list)
			{
				Picture picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
				OCarouselModel.OCarouselCategoryModel oCarouselCategoryModel = new OCarouselModel.OCarouselCategoryModel();
				OCarouselModel.OCarouselCategoryModel oCarouselCategoryModel2 = oCarouselCategoryModel;
				oCarouselCategoryModel2.Name = await _localizationService.GetLocalizedAsync(category, (Category x) => x.Name);
				OCarouselModel.OCarouselCategoryModel oCarouselCategoryModel3 = oCarouselCategoryModel;
				oCarouselCategoryModel3.SeName = await _urlRecordService.GetSeNameAsync(category);
				OCarouselModel.OCarouselCategoryModel oCarouselCategoryModel4 = oCarouselCategoryModel;
				PictureModel pictureModel = new PictureModel();
				PictureModel pictureModel2 = pictureModel;
				pictureModel2.ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSize)).Item1;
				PictureModel pictureModel3 = pictureModel;
				pictureModel3.FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Item1;
				PictureModel pictureModel4 = pictureModel;
				string title = ((picture == null || string.IsNullOrEmpty(picture.TitleAttribute)) ? string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), category.Name) : picture.TitleAttribute);
				pictureModel4.Title = title;
				PictureModel pictureModel5 = pictureModel;
				string alternateText = ((picture == null || string.IsNullOrEmpty(picture.AltAttribute)) ? string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), category.Name) : picture.AltAttribute);
				pictureModel5.AlternateText = alternateText;
				oCarouselCategoryModel4.PictureModel = pictureModel;
				listModel.Add(oCarouselCategoryModel);
			}
			return listModel;
		});
	}

	protected async Task<string> GetCarouselBackgroundImage(OCarousel carousel)
	{
		CacheKey key = _cacheKeyService.PrepareKeyForDefaultCache(NopStation.Plugin.Widgets.OCarousels.Infrastructure.Cache.ModelCacheEventConsumer.OCAROUSEL_BACKGROUND_PICTURE_MODEL_KEY, carousel, _storeContext.GetCurrentStoreAsync());
		return await _cacheManager.GetAsync(key, async () => await _pictureService.GetPictureUrlAsync(carousel.BackgroundPictureId));
	}

	protected static CarouselType GetCarouselType(DataSourceTypeEnum dataSource)
	{
		return dataSource switch
		{
			DataSourceTypeEnum.HomePageCategories => CarouselType.Category, 
			DataSourceTypeEnum.Manufacturers => CarouselType.Manufacturer, 
			_ => CarouselType.Product, 
		};
	}

	public async Task<OCarouselListModel> PrepareCarouselListModelAsync(IList<OCarousel> carousels)
	{
		ArgumentNullException.ThrowIfNull(carousels, "carousels");
		OCarouselListModel model = new OCarouselListModel();
		foreach (OCarousel carousel in carousels)
		{
			List<OCarouselListModel.OCarouselOverviewModel> oCarousels = model.OCarousels;
			OCarouselListModel.OCarouselOverviewModel oCarouselOverviewModel = new OCarouselListModel.OCarouselOverviewModel();
			OCarouselListModel.OCarouselOverviewModel oCarouselOverviewModel2 = oCarouselOverviewModel;
			oCarouselOverviewModel2.Title = await _localizationService.GetLocalizedAsync(carousel, (OCarousel x) => x.Title);
			oCarouselOverviewModel.DisplayTitle = carousel.DisplayTitle;
			oCarouselOverviewModel.CarouselType = GetCarouselType(carousel.DataSourceTypeEnum);
			oCarouselOverviewModel.Id = carousel.Id;
			oCarouselOverviewModel.ShowBackgroundPicture = carousel.ShowBackgroundPicture;
			OCarouselListModel.OCarouselOverviewModel oCarouselOverviewModel3 = oCarouselOverviewModel;
			string backgroundPictureUrl = ((!carousel.ShowBackgroundPicture) ? "" : (await GetCarouselBackgroundImage(carousel)));
			oCarouselOverviewModel3.BackgroundPictureUrl = backgroundPictureUrl;
			oCarousels.Add(oCarouselOverviewModel);
		}
		return model;
	}

	public async Task<OCarouselModel> PrepareCarouselModelAsync(OCarousel carousel)
	{
		ArgumentNullException.ThrowIfNull(carousel, "carousel");
		OCarouselModel oCarouselModel = new OCarouselModel
		{
			Id = carousel.Id,
			AutoPlay = carousel.AutoPlay
		};
		OCarouselModel oCarouselModel2 = oCarouselModel;
		oCarouselModel2.Rtl = (await _workContext.GetWorkingLanguageAsync()).Rtl;
		oCarouselModel.CustomCssClass = carousel.CustomCssClass;
		oCarouselModel.AutoPlayHoverPause = carousel.AutoPlayHoverPause;
		oCarouselModel.AutoPlayTimeout = carousel.AutoPlayTimeout;
		oCarouselModel.Center = carousel.Center;
		oCarouselModel.LazyLoad = carousel.LazyLoad;
		oCarouselModel.LazyLoadEager = carousel.LazyLoadEager;
		oCarouselModel.Loop = carousel.Loop;
		oCarouselModel.Nav = carousel.Nav;
		oCarouselModel.StartPosition = carousel.StartPosition;
		oCarouselModel.CarouselType = GetCarouselType(carousel.DataSourceTypeEnum);
		OCarouselModel model = oCarouselModel;
		if (carousel.ShowBackgroundPicture)
		{
			model.ShowBackgroundPicture = carousel.ShowBackgroundPicture;
			oCarouselModel = model;
			oCarouselModel.BackgroundPictureUrl = await GetCarouselBackgroundImage(carousel);
		}
		if (carousel.DisplayTitle)
		{
			model.DisplayTitle = true;
			oCarouselModel = model;
			oCarouselModel.Title = await _localizationService.GetLocalizedAsync(carousel, (OCarousel x) => x.Title);
		}
		switch (carousel.DataSourceTypeEnum)
		{
		case DataSourceTypeEnum.HomePageCategories:
			oCarouselModel = model;
			oCarouselModel.Categories = await PrepareCategoryListModelAsync(carousel);
			break;
		case DataSourceTypeEnum.HomePageProducts:
		{
			IList<Product> products4 = await (await (await _productService.GetAllProductsDisplayedOnHomepageAsync()).WhereAwait(async delegate(Product p)
			{
				return await _storeMappingService.AuthorizeAsync(p);
			}).ToListAsync()).Where((Product p) => _productService.ProductIsAvailable(p)).Take(carousel.NumberOfItemsToShow).ToListAsync();
			model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products4)).ToList();
			break;
		}
		case DataSourceTypeEnum.Manufacturers:
			oCarouselModel = model;
			oCarouselModel.Manufacturers = await PrepareManufacturerListModel(carousel);
			break;
		case DataSourceTypeEnum.NewProducts:
		{
			IProductService productService = _productService;
			IPagedList<Product> products2 = await productService.GetProductsMarkedAsNewAsync((await _storeContext.GetCurrentStoreAsync()).Id);
			oCarouselModel = model;
			oCarouselModel.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products2)).ToList();
			break;
		}
		case DataSourceTypeEnum.RecentlyViewedProducts:
		{
			IList<Product> products5 = await _recentlyViewedProductsService.GetRecentlyViewedProductsAsync(carousel.NumberOfItemsToShow);
			model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products5)).ToList();
			break;
		}
		case DataSourceTypeEnum.BestSellers:
		{
			CacheKey key2 = _cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.HomepageBestsellersIdsKey);
			List<BestsellersReportLine> source = await _cacheManager.GetAsync(key2, async delegate
			{
				IOrderReportService orderReportService = _orderReportService;
				DateTime? createdFromUtc = DateTime.UtcNow.AddDays(-30.0);
				int id = (await _storeContext.GetCurrentStoreAsync()).Id;
				int numberOfItemsToShow = carousel.NumberOfItemsToShow;
				return (await orderReportService.BestSellersReportAsync(0, 0, id, 0, createdFromUtc, null, null, null, null, 0, OrderByEnum.OrderByQuantity, 0, numberOfItemsToShow)).ToList();
			});
			IList<Product> products3 = (await (await _productService.GetProductsByIdsAsync(source.Select((BestsellersReportLine x) => x.ProductId).ToArray())).WhereAwait(async delegate(Product p)
			{
				return await _storeMappingService.AuthorizeAsync(p);
			}).ToListAsync()).Where((Product p) => _productService.ProductIsAvailable(p)).ToList();
			oCarouselModel = model;
			oCarouselModel.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products3)).ToList();
			break;
		}
		case DataSourceTypeEnum.CustomProducts:
		{
			CacheKey key = _cacheKeyService.PrepareKeyForDefaultCache(NopStation.Plugin.Widgets.OCarousels.Infrastructure.Cache.ModelCacheEventConsumer.OCAROUSEL_CUSTOMRODUCTIDS_MODEL_KEY, carousel.Id);
			int[] productIds = await _cacheManager.GetAsync(key, async () => (await _carouselService.GetOCarouselItemsByOCarouselIdAsync(carousel.Id)).Select((OCarouselItem ci) => ci.ProductId).ToArray());
			List<Product> products = (await (await _productService.GetProductsByIdsAsync(productIds)).Where((Product p) => p.Published).ToList().WhereAwait(async delegate(Product p)
			{
				return await _storeMappingService.AuthorizeAsync(p);
			})
				.ToListAsync()).Where((Product p) => _productService.ProductIsAvailable(p)).ToList();
			oCarouselModel = model;
			oCarouselModel.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
			break;
		}
		}
		return model;
	}
}
