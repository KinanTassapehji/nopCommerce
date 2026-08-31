using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.OCarousels.Domains;
using NopStation.Plugin.Widgets.OCarousels.Helpers;
using NopStation.Plugin.Widgets.OCarousels.Services;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Factories;

public class OCarouselModelFactory : IOCarouselModelFactory
{
	private readonly IStoreContext _storeContext;

	private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

	private readonly ILocalizedModelFactory _localizedModelFactory;

	private readonly IBaseAdminModelFactory _baseAdminModelFactory;

	private readonly ILocalizationService _localizationService;

	private readonly IUrlRecordService _urlRecordService;

	private readonly IOCarouselService _carouselService;

	private readonly IProductService _productService;

	private readonly IPictureService _pictureService;

	private readonly ISettingService _settingService;

	private readonly IDateTimeHelper _dateTimeHelper;

	public OCarouselModelFactory(IStoreContext storeContext, IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory, ILocalizedModelFactory localizedModelFactory, IBaseAdminModelFactory baseAdminModelFactory, ILocalizationService localizationService, IUrlRecordService urlRecordService, IOCarouselService carouselService, IProductService productService, IPictureService pictureService, ISettingService settingService, IDateTimeHelper dateTimeHelper)
	{
		_storeContext = storeContext;
		_storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
		_localizedModelFactory = localizedModelFactory;
		_baseAdminModelFactory = baseAdminModelFactory;
		_localizationService = localizationService;
		_urlRecordService = urlRecordService;
		_carouselService = carouselService;
		_productService = productService;
		_pictureService = pictureService;
		_settingService = settingService;
		_dateTimeHelper = dateTimeHelper;
	}

	protected async Task PrepareCustomWidgetZonesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
	{
		ArgumentNullException.ThrowIfNull(items, "items");
		foreach (SelectListItem customWidgetZoneSelect in OCarouselHelper.GetCustomWidgetZoneSelectList())
		{
			items.Add(customWidgetZoneSelect);
		}
		if (withSpecialDefaultItem)
		{
			SelectListItem selectListItem = new SelectListItem();
			SelectListItem selectListItem2 = selectListItem;
			selectListItem2.Text = await _localizationService.GetResourceAsync("Admin.Common.All");
			selectListItem.Value = "0";
			items.Insert(0, selectListItem);
		}
	}

	protected async Task PrepareDataSourceTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
	{
		ArgumentNullException.ThrowIfNull(items, "items");
		foreach (SelectListItem item in (await DataSourceTypeEnum.BestSellers.ToSelectListAsync(markCurrentAsSelected: false)).ToList())
		{
			items.Add(item);
		}
		if (withSpecialDefaultItem)
		{
			SelectListItem selectListItem = new SelectListItem();
			SelectListItem selectListItem2 = selectListItem;
			selectListItem2.Text = await _localizationService.GetResourceAsync("Admin.Common.All");
			selectListItem.Value = "0";
			items.Insert(0, selectListItem);
		}
	}

	protected async Task PrepareActiveOptionsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
	{
		ArgumentNullException.ThrowIfNull(items, "items");
		IList<SelectListItem> list = items;
		SelectListItem selectListItem = new SelectListItem();
		SelectListItem selectListItem2 = selectListItem;
		selectListItem2.Text = await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.OCarousels.List.SearchActive.Active");
		selectListItem.Value = "1";
		list.Add(selectListItem);
		list = items;
		selectListItem2 = new SelectListItem();
		selectListItem = selectListItem2;
		selectListItem.Text = await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.OCarousels.List.SearchActive.Inactive");
		selectListItem2.Value = "2";
		list.Add(selectListItem2);
		if (withSpecialDefaultItem)
		{
			list = items;
			selectListItem = new SelectListItem();
			selectListItem2 = selectListItem;
			selectListItem2.Text = await _localizationService.GetResourceAsync("Admin.Common.All");
			selectListItem.Value = "0";
			list.Insert(0, selectListItem);
		}
	}

	public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
	{
		int storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
		OCarouselSettings settings = await _settingService.LoadSettingAsync<OCarouselSettings>(storeId);
		ConfigurationModel model = settings.ToSettingsModel<ConfigurationModel>();
		model.ActiveStoreScopeConfiguration = storeId;
		if (storeId <= 0)
		{
			return model;
		}
		ConfigurationModel configurationModel = model;
		configurationModel.EnableOCarousel_OverrideForStore = await _settingService.SettingExistsAsync(settings, (OCarouselSettings x) => x.EnableOCarousel, storeId);
		return model;
	}

	public virtual async Task<OCarouselSearchModel> PrepareOCarouselSearchModelAsync(OCarouselSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		await PrepareCustomWidgetZonesAsync(searchModel.AvailableWidgetZones);
		await PrepareDataSourceTypesAsync(searchModel.AvailableDataSources);
		await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions);
		await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);
		return searchModel;
	}

	public virtual async Task<OCarouselListModel> PrepareOCarouselListModelAsync(OCarouselSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		List<int> widgetZoneIds = ((searchModel.SearchWidgetZones?.Contains(0) ?? true) ? null : searchModel.SearchWidgetZones.ToList());
		List<int> dataSources = ((searchModel.SearchDataSources?.Contains(0) ?? true) ? null : searchModel.SearchDataSources.ToList());
		bool? active = searchModel.SearchActiveId switch
		{
			1 => true, 
			2 => false, 
			_ => null, 
		};
		IPagedList<OCarousel> carousels = await _carouselService.GetAllCarouselsAsync(widgetZoneIds, dataSources, searchModel.SearchStoreId, active, searchModel.Page - 1, searchModel.PageSize);
		return await new OCarouselListModel().PrepareToGridAsync(searchModel, carousels, () => carousels.SelectAwait<OCarousel, OCarouselModel>(async (OCarousel carousel) => await PrepareOCarouselModelAsync(null, carousel, excludeProperties: true)));
	}

	public async Task<OCarouselModel> PrepareOCarouselModelAsync(OCarouselModel model, OCarousel carousel, bool excludeProperties = false)
	{
		Func<OCarouselLocalizedModel, int, Task> localizedModelConfiguration = null;
		if (carousel != null)
		{
			if (model == null)
			{
				model = carousel.ToModel<OCarouselModel>();
				OCarouselModel oCarouselModel = model;
				oCarouselModel.DataSourceTypeStr = await _localizationService.GetLocalizedEnumAsync(carousel.DataSourceTypeEnum);
				model.WidgetZoneStr = OCarouselHelper.GetCustomWidgetZone(carousel.WidgetZoneId);
				oCarouselModel = model;
				oCarouselModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(carousel.CreatedOnUtc, DateTimeKind.Utc);
				oCarouselModel = model;
				oCarouselModel.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(carousel.UpdatedOnUtc, DateTimeKind.Utc);
			}
			if (!excludeProperties)
			{
				model.OCarouselItemSearchModel = new OCarouselItemSearchModel
				{
					OCarouselId = carousel.Id
				};
				localizedModelConfiguration = async delegate(OCarouselLocalizedModel locale, int languageId)
				{
					locale.Title = await _localizationService.GetLocalizedAsync(carousel, (OCarousel entity) => entity.Title, languageId, returnDefaultValue: false, ensureTwoPublishedLanguages: false);
				};
			}
		}
		if (!excludeProperties)
		{
			OCarouselModel oCarouselModel = model;
			oCarouselModel.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
			await PrepareCustomWidgetZonesAsync(model.AvailableWidgetZones, withSpecialDefaultItem: false);
			await PrepareDataSourceTypesAsync(model.AvailableDataSources, withSpecialDefaultItem: false);
			await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, carousel, excludeProperties);
		}
		return model;
	}

	public async Task<OCarouselItemListModel> PrepareOCarouselItemListModelAsync(OCarouselItemSearchModel searchModel, OCarousel carousel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		ArgumentNullException.ThrowIfNull(carousel, "carousel");
		IPagedList<OCarouselItem> carouselItems = await _carouselService.GetOCarouselItemsByOCarouselIdAsync(carousel.Id, searchModel.Page - 1, searchModel.PageSize);
		return await new OCarouselItemListModel().PrepareToGridAsync(searchModel, carouselItems, () => carouselItems.SelectAwait<OCarouselItem, OCarouselItemModel>(async delegate(OCarouselItem carouselItem)
		{
			Product product = await _productService.GetProductByIdAsync(carouselItem.ProductId);
			Picture picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
			OCarouselItemModel carouselItemModel = carouselItem.ToModel<OCarouselItemModel>();
			carouselItemModel.ProductName = product.Name;
			OCarouselItemModel oCarouselItemModel = carouselItemModel;
			oCarouselItemModel.PictureUrl = await _pictureService.GetPictureUrlAsync(picture?.Id ?? 0, 75);
			return carouselItemModel;
		}));
	}

	public async Task<AddProductToCarouselSearchModel> PrepareAddProductToOCarouselSearchModelAsync(AddProductToCarouselSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);
		await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);
		await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);
		await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);
		await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);
		searchModel.SetPopupGridPageSize();
		return searchModel;
	}

	public async Task<AddProductToCarouselListModel> PrepareAddProductToOCarouselListModelAsync(AddProductToCarouselSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		IPagedList<Product> products = await _productService.SearchProductsAsync(categoryIds: new List<int> { searchModel.SearchCategoryId }, manufacturerIds: new List<int> { searchModel.SearchManufacturerId }, storeId: searchModel.SearchStoreId, vendorId: searchModel.SearchVendorId, productType: (searchModel.SearchProductTypeId > 0) ? new ProductType?((ProductType)searchModel.SearchProductTypeId) : ((ProductType?)null), keywords: searchModel.SearchProductName, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, warehouseId: 0, visibleIndividuallyOnly: false, excludeFeaturedProducts: false, priceMin: null, priceMax: null, productTagId: 0, searchDescriptions: false, searchManufacturerPartNumber: true, searchSku: true, searchProductTags: false, languageId: 0, orderBy: ProductSortingEnum.Position, showHidden: true);
		return await new AddProductToCarouselListModel().PrepareToGridAsync(searchModel, products, () => products.SelectAwait<Product, ProductModel>(async delegate(Product product)
		{
			ProductModel productModel = product.ToModel<ProductModel>();
			ProductModel productModel2 = productModel;
			productModel2.SeName = await _urlRecordService.GetSeNameAsync(product, 0, returnDefaultValue: true, ensureTwoPublishedLanguages: false);
			return productModel;
		}));
	}
}
