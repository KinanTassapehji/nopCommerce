using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductTabs.Domains;
using NopStation.Plugin.Widgets.ProductTabs.Helpers;
using NopStation.Plugin.Widgets.ProductTabs.Services;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Factories;

public class ProductTabModelFactory : IProductTabModelFactory
{
	private readonly IProductTabService _productTabService;

	private readonly ILocalizationService _localizationService;

	private readonly IProductService _productService;

	private readonly IPictureService _pictureService;

	private readonly IBaseAdminModelFactory _baseAdminModelFactory;

	private readonly IUrlRecordService _urlRecordService;

	private readonly IDateTimeHelper _dateTimeHelper;

	private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

	private readonly ILocalizedModelFactory _localizedModelFactory;

	public ProductTabModelFactory(IProductTabService productTabService, ILocalizationService localizationService, IProductService productService, IPictureService pictureService, IBaseAdminModelFactory baseAdminModelFactory, IUrlRecordService urlRecordService, IDateTimeHelper dateTimeHelper, IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory, ILocalizedModelFactory localizedModelFactory)
	{
		_productTabService = productTabService;
		_localizationService = localizationService;
		_productService = productService;
		_pictureService = pictureService;
		_baseAdminModelFactory = baseAdminModelFactory;
		_urlRecordService = urlRecordService;
		_dateTimeHelper = dateTimeHelper;
		_storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
		_localizedModelFactory = localizedModelFactory;
	}

	protected async Task PrepareCustomWidgetZonesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
	{
		ArgumentNullException.ThrowIfNull(items, "items");
		foreach (SelectListItem customWidgetZoneSelect in ProductTabHelper.GetCustomWidgetZoneSelectList())
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

	protected async Task PrepareActiveOptionsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
	{
		ArgumentNullException.ThrowIfNull(items, "items");
		IList<SelectListItem> list = items;
		SelectListItem selectListItem = new SelectListItem();
		SelectListItem selectListItem2 = selectListItem;
		selectListItem2.Text = await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabs.List.SearchActive.Active");
		selectListItem.Value = "1";
		list.Add(selectListItem);
		list = items;
		selectListItem2 = new SelectListItem();
		selectListItem = selectListItem2;
		selectListItem.Text = await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabs.List.SearchActive.Inactive");
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

	public async Task<ProductTabSearchModel> PrepareOCarouselSearchModelAsync(ProductTabSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		await PrepareCustomWidgetZonesAsync(searchModel.AvailableWidgetZones);
		await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions);
		await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);
		return searchModel;
	}

	public virtual async Task<ProductTabListModel> PrepareProductTabListModelAsync(ProductTabSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		List<int> widgetZoneIds = ((searchModel.SearchWidgetZones?.Contains(0) ?? true) ? null : searchModel.SearchWidgetZones.ToList());
		bool? active = null;
		if (searchModel.SearchActiveId == 1)
		{
			active = true;
		}
		else if (searchModel.SearchActiveId == 2)
		{
			active = false;
		}
		IPagedList<ProductTab> productTabs = await _productTabService.GetAllProductTabsAsync(widgetZoneIds, hasItemsOnly: false, searchModel.SearchStoreId, active, searchModel.Page - 1, searchModel.PageSize);
		return await new ProductTabListModel().PrepareToGridAsync(searchModel, productTabs, () => productTabs.SelectAwait<ProductTab, ProductTabModel>(async (ProductTab productTab) => await PrepareProductTabModelAsync(null, productTab, excludeProperties: true)));
	}

	public async Task<ProductTabModel> PrepareProductTabModelAsync(ProductTabModel model, ProductTab productTab, bool excludeProperties = false)
	{
		Func<ProductTabLocalizedModel, int, Task> localizedModelConfiguration = null;
		if (productTab != null)
		{
			if (model == null)
			{
				model = productTab.ToModel<ProductTabModel>();
				model.WidgetZoneStr = ProductTabHelper.GetCustomWidgetZone(productTab.WidgetZoneId);
				ProductTabModel productTabModel = model;
				productTabModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(productTab.CreatedOnUtc, DateTimeKind.Utc);
				productTabModel = model;
				productTabModel.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(productTab.UpdatedOnUtc, DateTimeKind.Utc);
			}
			if (!excludeProperties)
			{
				model.ProductTabItemSearchModel = new ProductTabItemSearchModel
				{
					ProductTabId = productTab.Id
				};
				localizedModelConfiguration = async delegate(ProductTabLocalizedModel locale, int languageId)
				{
					ProductTabLocalizedModel productTabLocalizedModel = locale;
					productTabLocalizedModel.Name = await _localizationService.GetLocalizedAsync(productTab, (ProductTab entity) => entity.Name, languageId, returnDefaultValue: false, ensureTwoPublishedLanguages: false);
					productTabLocalizedModel = locale;
					productTabLocalizedModel.TabTitle = await _localizationService.GetLocalizedAsync(productTab, (ProductTab entity) => entity.TabTitle, languageId, returnDefaultValue: false, ensureTwoPublishedLanguages: false);
				};
			}
		}
		if (!excludeProperties)
		{
			ProductTabModel productTabModel = model;
			productTabModel.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
			await PrepareCustomWidgetZonesAsync(model.AvailableWidgetZones, withSpecialDefaultItem: false);
			await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, productTab, excludeProperties);
		}
		return model;
	}

	public async Task<ProductTabItemListModel> PrepareProductTabItemListModelAsync(ProductTabItemSearchModel searchModel, ProductTab productTab)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		ArgumentNullException.ThrowIfNull(productTab, "productTab");
		IPagedList<ProductTabItem> productTabItems = (from x in _productTabService.GetProductTabItemsByProductTabId(productTab.Id)
			orderby x.DisplayOrder
			select x).ToList().ToPagedList(searchModel);
		return await new ProductTabItemListModel().PrepareToGridAsync(searchModel, productTabItems, () => productTabItems.SelectAwait<ProductTabItem, ProductTabItemModel>(async delegate(ProductTabItem productTabItem)
		{
			ProductTabItemModel model = new ProductTabItemModel
			{
				Id = productTabItem.Id,
				DisplayOrder = productTabItem.DisplayOrder,
				Name = productTabItem.Name,
				ProductTabId = productTabItem.ProductTabId
			};
			return await PrepareProductTabItemModelAsync(model, productTabItem, productTab, excludeProperties: true);
		}));
	}

	public async Task<ProductTabItemModel> PrepareProductTabItemModelAsync(ProductTabItemModel model, ProductTabItem productTabItem, ProductTab productTab, bool excludeProperties = false)
	{
		Func<ProductTabItemLocalizedModel, int, Task> configure = null;
		if (productTabItem != null)
		{
			if (!excludeProperties)
			{
				_ = (Func<ProductTabItemLocalizedModel, int, Task>)async delegate(ProductTabItemLocalizedModel locale, int languageId)
				{
					locale.Name = await _localizationService.GetLocalizedAsync(productTabItem, (ProductTabItem entity) => entity.Name, languageId, returnDefaultValue: false, ensureTwoPublishedLanguages: false);
				};
			}
		}
		else if (!excludeProperties)
		{
			try
			{
				model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(configure);
			}
			catch (Exception ex)
			{
				_ = ex.InnerException.Message;
			}
		}
		return model;
	}

	public async Task<ProductTabItemProductListModel> PrepareProductTabItemProductListModelAsync(ProductTabItemProductSearchModel searchModel, ProductTabItem productTabItem)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		ArgumentNullException.ThrowIfNull(productTabItem, "productTabItem");
		IPagedList<ProductTabItemProduct> productTabItemProducts = (from x in _productTabService.GetProductTabItemProductsByProductTabItemId(productTabItem.Id)
			orderby x.DisplayOrder
			select x).ToList().ToPagedList(searchModel);
		return await new ProductTabItemProductListModel().PrepareToGridAsync(searchModel, productTabItemProducts, () => productTabItemProducts.SelectAwait<ProductTabItemProduct, ProductTabItemProductModel>(async (ProductTabItemProduct product) => await PrepareProductTabItemProductModelAsync(null, product, productTabItem)));
	}

	protected async Task<ProductTabItemProductModel> PrepareProductTabItemProductModelAsync(ProductTabItemProductModel model, ProductTabItemProduct itemProduct, ProductTabItem productTabItem)
	{
		if (itemProduct != null && model == null)
		{
			model = itemProduct.ToModel<ProductTabItemProductModel>();
			model.ProductName = (await _productService.GetProductByIdAsync(itemProduct.ProductId))?.Name;
		}
		return model;
	}

	public async Task<AddProductToProductTabItemSearchModel> PrepareAddProductToProductTabItemSearchModelAsync(AddProductToProductTabItemSearchModel searchModel)
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

	public async Task<AddProductToProductTabItemListModel> PrepareAddProductToProductTabItemListModelAsync(AddProductToProductTabItemSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		IPagedList<Product> products = await _productService.SearchProductsAsync(categoryIds: new List<int> { searchModel.SearchCategoryId }, manufacturerIds: new List<int> { searchModel.SearchManufacturerId }, storeId: searchModel.SearchStoreId, vendorId: searchModel.SearchVendorId, productType: (searchModel.SearchProductTypeId > 0) ? new ProductType?((ProductType)searchModel.SearchProductTypeId) : ((ProductType?)null), keywords: searchModel.SearchProductName, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, warehouseId: 0, visibleIndividuallyOnly: false, excludeFeaturedProducts: false, priceMin: null, priceMax: null, productTagId: 0, searchDescriptions: false, searchManufacturerPartNumber: true, searchSku: true, searchProductTags: false, languageId: 0, orderBy: ProductSortingEnum.Position, showHidden: true);
		return await new AddProductToProductTabItemListModel().PrepareToGridAsync(searchModel, products, () => products.SelectAwait<Product, ProductModel>(async delegate(Product product)
		{
			ProductModel productModel = product.ToModel<ProductModel>();
			ProductModel productModel2 = productModel;
			productModel2.SeName = await _urlRecordService.GetSeNameAsync(product, 0, returnDefaultValue: true, ensureTwoPublishedLanguages: false);
			return productModel;
		}));
	}
}
