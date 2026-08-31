using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductTabs.Domains;
using NopStation.Plugin.Widgets.ProductTabs.Services;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Controllers;

public class ProductTabController : NopStationAdminController
{
	private readonly ISettingHelper<ProductTabSettings, ConfigurationModel> _settingHelper;

	private readonly ILocalizationService _localizationService;

	private readonly INotificationService _notificationService;

	private readonly IProductTabService _productTabService;

	private readonly IProductTabModelFactory _productTabModelFactory;

	private readonly IProductService _productService;

	private readonly IStoreMappingService _storeMappingService;

	private readonly IStoreService _storeService;

	private readonly ILocalizedEntityService _localizedEntityService;

	public ProductTabController(ISettingHelper<ProductTabSettings, ConfigurationModel> settingHelper, ILocalizationService localizationService, INotificationService notificationService, IProductTabService productTabService, IProductTabModelFactory productTabModelFactory, IProductService productService, IStoreMappingService storeMappingService, IStoreService storeService, ILocalizedEntityService localizedEntityService)
	{
		_settingHelper = settingHelper;
		_localizationService = localizationService;
		_notificationService = notificationService;
		_productTabService = productTabService;
		_productTabModelFactory = productTabModelFactory;
		_productService = productService;
		_storeMappingService = storeMappingService;
		_storeService = storeService;
		_localizedEntityService = localizedEntityService;
	}

	protected virtual async Task SaveStoreMappingsAsync(ProductTab carousel, ProductTabModel model)
	{
		carousel.LimitedToStores = model.SelectedStoreIds.Any();
		IList<StoreMapping> existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(carousel);
		foreach (Store store in await _storeService.GetAllStoresAsync())
		{
			if (model.SelectedStoreIds.Contains(store.Id))
			{
				if (existingStoreMappings.Count((StoreMapping sm) => sm.StoreId == store.Id) == 0)
				{
					await _storeMappingService.InsertStoreMappingAsync(carousel, store.Id);
				}
				continue;
			}
			StoreMapping storeMapping = existingStoreMappings.FirstOrDefault((StoreMapping sm) => sm.StoreId == store.Id);
			if (storeMapping != null)
			{
				await _storeMappingService.DeleteStoreMappingAsync(storeMapping);
			}
		}
	}

	protected virtual async Task UpdateLocalesAsync(ProductTab productTab, ProductTabModel model)
	{
		foreach (ProductTabLocalizedModel localized in model.Locales)
		{
			await _localizedEntityService.SaveLocalizedValueAsync(productTab, (ProductTab x) => x.Name, localized.Name, localized.LanguageId);
			await _localizedEntityService.SaveLocalizedValueAsync(productTab, (ProductTab x) => x.TabTitle, localized.TabTitle, localized.LanguageId);
		}
	}

	protected virtual async Task UpdateLocalesAsync(ProductTabItem productTabItem, ProductTabItemModel model)
	{
		foreach (ProductTabItemLocalizedModel locale in model.Locales)
		{
			await _localizedEntityService.SaveLocalizedValueAsync(productTabItem, (ProductTabItem x) => x.Name, locale.Name, locale.LanguageId);
		}
	}

	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		return View(await _settingHelper.PrepareConfigurationModelAsync(null));
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		await _settingHelper.SaveConfigurationModelAsync(model, null, true);
		return RedirectToAction("Configure");
	}

	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> List()
	{
		return View(await _productTabModelFactory.PrepareOCarouselSearchModelAsync(new ProductTabSearchModel()));
	}

	[HttpPost]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> List(ProductTabSearchModel searchModel)
	{
		return Json(await _productTabModelFactory.PrepareProductTabListModelAsync(searchModel));
	}

	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Create()
	{
		return View(await _productTabModelFactory.PrepareProductTabModelAsync(new ProductTabModel(), null));
	}

	[EditAccess(false)]
	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Create(ProductTabModel model, bool continueEditing)
	{
		if (base.ModelState.IsValid)
		{
			ProductTab productTab = model.ToEntity<ProductTab>();
			model.CreatedOn = DateTime.UtcNow;
			model.UpdatedOn = DateTime.UtcNow;
			productTab.CreatedOnUtc = DateTime.UtcNow;
			productTab.UpdatedOnUtc = DateTime.UtcNow;
			await _productTabService.InsertProductTabAsync(productTab);
			await UpdateLocalesAsync(productTab, model);
			await SaveStoreMappingsAsync(productTab, model);
			await _productTabService.UpdateProductTabAsync(productTab);
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabs.Created"));
			return continueEditing ? RedirectToAction("Edit", new
			{
				id = productTab.Id
			}) : RedirectToAction("List");
		}
		model = await _productTabModelFactory.PrepareProductTabModelAsync(model, null);
		return View(model);
	}

	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Edit(int id)
	{
		ProductTab productTab = await _productTabService.GetProductTabByIdAsync(id);
		if (productTab == null || productTab.Deleted)
		{
			return RedirectToAction("List");
		}
		return View(await _productTabModelFactory.PrepareProductTabModelAsync(null, productTab));
	}

	[EditAccess(false)]
	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Edit(ProductTabModel model, bool continueEditing)
	{
		ProductTab productTab = await _productTabService.GetProductTabByIdAsync(model.Id);
		if (productTab == null || productTab.Deleted)
		{
			return RedirectToAction("List");
		}
		if (base.ModelState.IsValid)
		{
			model.UpdatedOn = DateTime.UtcNow;
			productTab = model.ToEntity(productTab);
			productTab.UpdatedOnUtc = DateTime.UtcNow;
			await _productTabService.UpdateProductTabAsync(productTab);
			await UpdateLocalesAsync(productTab, model);
			await SaveStoreMappingsAsync(productTab, model);
			await _productTabService.UpdateProductTabAsync(productTab);
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabs.Updated"));
			return continueEditing ? RedirectToAction("Edit", new
			{
				id = model.Id
			}) : RedirectToAction("List");
		}
		model = await _productTabModelFactory.PrepareProductTabModelAsync(model, productTab);
		return View(model);
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Delete(ProductTabModel model)
	{
		ProductTab productTab = await _productTabService.GetProductTabByIdAsync(model.Id);
		if (productTab == null || productTab.Deleted)
		{
			return RedirectToAction("List");
		}
		await _productTabService.DeleteProductTabAsync(productTab);
		return RedirectToAction("List");
	}

	[HttpPost]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ItemList(ProductTabItemSearchModel searchModel)
	{
		ProductTab productTab = await _productTabService.GetProductTabByIdAsync(searchModel.ProductTabId);
		if (productTab == null || productTab.Deleted)
		{
			return new NullJsonResult();
		}
		return Json(await _productTabModelFactory.PrepareProductTabItemListModelAsync(searchModel, productTab));
	}

	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ItemCreate(int productTabId)
	{
		ProductTab productTab = await _productTabService.GetProductTabByIdAsync(productTabId);
		if (productTab == null || productTab.Deleted)
		{
			return RedirectToAction("List");
		}
		ProductTabItemModel productTabItemModel = new ProductTabItemModel();
		if (productTab != null)
		{
			productTabItemModel.Name = productTab.Name;
			productTabItemModel.ProductTabId = productTab.Id;
		}
		return View(await _productTabModelFactory.PrepareProductTabItemModelAsync(productTabItemModel, null, productTab));
	}

	[EditAccess(false)]
	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ItemCreate(ProductTabItemModel model, bool continueEditing)
	{
		ProductTab productTab = await _productTabService.GetProductTabByIdAsync(model.ProductTabId);
		if (productTab == null || productTab.Deleted)
		{
			return RedirectToAction("List");
		}
		if (base.ModelState.IsValid)
		{
			try
			{
				ProductTabItem tabItem = new ProductTabItem
				{
					Name = model.Name,
					DisplayOrder = model.DisplayOrder,
					ProductTabId = model.ProductTabId
				};
				await _productTabService.InsertProductTabItemAsync(tabItem);
				await UpdateLocalesAsync(tabItem, model);
				INotificationService notificationService = _notificationService;
				notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabItems.Created"));
				return continueEditing ? RedirectToAction("ItemEdit", new
				{
					id = tabItem.Id
				}) : RedirectToAction("Edit", new
				{
					id = productTab.Id
				});
			}
			catch (Exception ex)
			{
				_ = ex.InnerException.Message;
			}
		}
		model = await _productTabModelFactory.PrepareProductTabItemModelAsync(model, null, productTab);
		return View(model);
	}

	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ItemEdit(int id)
	{
		ProductTabItem productTabItem = await _productTabService.GetProductTabItemByIdAsync(id);
		if (productTabItem == null)
		{
			return RedirectToAction("List");
		}
		ProductTabItemModel productTabItemModel = new ProductTabItemModel();
		productTabItemModel.Id = id;
		productTabItemModel.Name = productTabItem.Name;
		productTabItemModel.DisplayOrder = productTabItem.DisplayOrder;
		productTabItemModel.ProductTabId = productTabItem.ProductTabId;
		return View(await _productTabModelFactory.PrepareProductTabItemModelAsync(productTabItemModel, productTabItem, productTabItem.ProductTab));
	}

	[EditAccess(false)]
	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ItemEdit(ProductTabItemModel model, bool continueEditing)
	{
		ProductTab productTab = await _productTabService.GetProductTabByIdAsync(model.ProductTabId);
		if (productTab == null || productTab.Deleted)
		{
			return RedirectToAction("List");
		}
		ProductTabItem productTabItem = await _productTabService.GetProductTabItemByIdAsync(model.Id);
		if (productTabItem == null)
		{
			return RedirectToAction("Edit", new
			{
				id = productTab.Id
			});
		}
		if (base.ModelState.IsValid)
		{
			productTabItem = model.ToEntity(productTabItem);
			await _productTabService.UpdateProductTabAsync(productTab);
			await _productTabService.UpdateProductTabItemAsync(productTabItem);
			await UpdateLocalesAsync(productTabItem, model);
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.ProductTabItems.Updated"));
			return continueEditing ? RedirectToAction("ItemEdit", new
			{
				id = model.Id
			}) : RedirectToAction("Edit", new
			{
				id = productTab.Id
			});
		}
		model = await _productTabModelFactory.PrepareProductTabItemModelAsync(model, productTabItem, productTab);
		return View(model);
	}

	[EditAccessAjax(false)]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ItemDelete(int id)
	{
		ProductTabItem productTabItem = await _productTabService.GetProductTabItemByIdAsync(id);
		if (productTabItem == null)
		{
			return new NullJsonResult();
		}
		await _productTabService.DeleteProductTabItemAsync(productTabItem);
		return new NullJsonResult();
	}

	[EditAccessAjax(false)]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ItemProductDelete(int id)
	{
		ProductTabItemProduct productTabItemProduct = await _productTabService.GetProductTabItemProductByIdAsync(id);
		if (productTabItemProduct == null)
		{
			return new NullJsonResult();
		}
		await _productTabService.DeleteProductTabItemProductAsync(productTabItemProduct);
		return new NullJsonResult();
	}

	[EditAccessAjax(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ItemProductUpdate(ProductTabItemProductModel model)
	{
		ProductTabItemProduct productTabItemProduct = await _productTabService.GetProductTabItemProductByIdAsync(model.Id);
		if (productTabItemProduct == null)
		{
			return new NullJsonResult();
		}
		productTabItemProduct.DisplayOrder = model.DisplayOrder;
		await _productTabService.UpdateProductTabItemProductAsync(productTabItemProduct);
		return new NullJsonResult();
	}

	[HttpPost]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ItemProductList(ProductTabItemProductSearchModel searchModel)
	{
		ProductTabItem productTabItem = await _productTabService.GetProductTabItemByIdAsync(searchModel.ProductTabItemId);
		if (productTabItem == null)
		{
			return new NullJsonResult();
		}
		return Json(await _productTabModelFactory.PrepareProductTabItemProductListModelAsync(searchModel, productTabItem));
	}

	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ProductAddPopup(int productTabItemId)
	{
		if (await _productTabService.GetProductTabItemByIdAsync(productTabItemId) == null)
		{
			throw new ArgumentException("No product tab item found with the specified id");
		}
		return View(await _productTabModelFactory.PrepareAddProductToProductTabItemSearchModelAsync(new AddProductToProductTabItemSearchModel()));
	}

	[HttpPost]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ProductAddPopupList(AddProductToProductTabItemSearchModel searchModel)
	{
		return Json(await _productTabModelFactory.PrepareAddProductToProductTabItemListModelAsync(searchModel));
	}

	[EditAccess(false)]
	[HttpPost]
	[FormValueRequired(new string[] { "save" })]
	[CheckPermission("ManageNopStationProductTab", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ProductAddPopup(AddProductToProductTabItemModel model)
	{
		ProductTabItem productTabItem = (await _productTabService.GetProductTabItemByIdAsync(model.ProductTabItemId)) ?? throw new ArgumentException("No product tab item found with the specified id");
		List<ProductTabItemProduct> itemProducts = productTabItem.ProductTabItemProducts.ToList();
		IList<Product> list = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
		if (list.Any())
		{
			foreach (Product product in list)
			{
				if (!itemProducts.Any((ProductTabItemProduct x) => x.ProductId == product.Id))
				{
					ProductTabItemProduct productTabItemProduct = new ProductTabItemProduct
					{
						ProductTabItemId = productTabItem.Id,
						DisplayOrder = 0,
						ProductId = product.Id
					};
					await _productTabService.InsertProductTabItemProductAsync(productTabItemProduct);
				}
			}
		}
		base.ViewBag.RefreshPage = true;
		return View(new AddProductToProductTabItemSearchModel());
	}
}
