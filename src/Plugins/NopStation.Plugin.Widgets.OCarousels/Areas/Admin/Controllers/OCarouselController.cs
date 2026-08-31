using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
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
using NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.OCarousels.Domains;
using NopStation.Plugin.Widgets.OCarousels.Services;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Controllers;

public class OCarouselController : NopStationAdminController
{
	private readonly IStoreContext _storeContext;

	private readonly ILocalizedEntityService _localizedEntityService;

	private readonly IOCarouselModelFactory _carouselModelFactory;

	private readonly ILocalizationService _localizationService;

	private readonly INotificationService _notificationService;

	private readonly IStoreMappingService _storeMappingService;

	private readonly IOCarouselService _carouselService;

	private readonly ISettingService _settingService;

	private readonly IProductService _productService;

	private readonly IStoreService _storeService;

	private readonly ISettingHelper<OCarouselSettings, ConfigurationModel> _settingHelper;

	public OCarouselController(IStoreContext storeContext, ILocalizedEntityService localizedEntityService, IOCarouselModelFactory carouselModelFactory, ILocalizationService localizationService, INotificationService notificationService, IStoreMappingService storeMappingService, IOCarouselService carouselService, ISettingService settingService, IProductService productService, IStoreService storeService, ISettingHelper<OCarouselSettings, ConfigurationModel> settingHelper)
	{
		_storeContext = storeContext;
		_localizedEntityService = localizedEntityService;
		_carouselModelFactory = carouselModelFactory;
		_localizationService = localizationService;
		_notificationService = notificationService;
		_storeMappingService = storeMappingService;
		_carouselService = carouselService;
		_settingService = settingService;
		_productService = productService;
		_storeService = storeService;
		_settingHelper = settingHelper;
	}

	protected virtual async Task SaveStoreMappingsAsync(OCarousel carousel, OCarouselModel model)
	{
		carousel.LimitedToStores = model.SelectedStoreIds.Count > 0;
		IList<StoreMapping> existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(carousel);
		foreach (Store store in await _storeService.GetAllStoresAsync())
		{
			if (model.SelectedStoreIds.Contains(store.Id))
			{
				if (!existingStoreMappings.Any((StoreMapping sm) => sm.StoreId == store.Id))
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

	protected virtual async Task UpdateLocalesAsync(OCarousel oCarousel, OCarouselModel model)
	{
		foreach (OCarouselLocalizedModel locale in model.Locales)
		{
			await _localizedEntityService.SaveLocalizedValueAsync(oCarousel, (OCarousel x) => x.Title, locale.Title, locale.LanguageId);
		}
	}

	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		return View(await _settingHelper.PrepareConfigurationModelAsync(null));
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		await _settingHelper.SaveConfigurationModelAsync(model, null, true);
		return RedirectToAction("Configure");
	}

	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> List()
	{
		return View(await _carouselModelFactory.PrepareOCarouselSearchModelAsync(new OCarouselSearchModel()));
	}

	[HttpPost]
	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> List(OCarouselSearchModel searchModel)
	{
		return Json(await _carouselModelFactory.PrepareOCarouselListModelAsync(searchModel));
	}

	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Create()
	{
		return View(await _carouselModelFactory.PrepareOCarouselModelAsync(new OCarouselModel(), null));
	}

	[EditAccess(false)]
	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Create(OCarouselModel model, bool continueEditing)
	{
		if (base.ModelState.IsValid)
		{
			OCarousel carousel = model.ToEntity<OCarousel>();
			carousel.CreatedOnUtc = DateTime.UtcNow;
			carousel.UpdatedOnUtc = DateTime.UtcNow;
			await _carouselService.InsertCarouselAsync(carousel);
			await UpdateLocalesAsync(carousel, model);
			await SaveStoreMappingsAsync(carousel, model);
			await _carouselService.UpdateCarouselAsync(carousel);
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Created"));
			return continueEditing ? RedirectToAction("Edit", new
			{
				id = carousel.Id
			}) : RedirectToAction("List");
		}
		model = await _carouselModelFactory.PrepareOCarouselModelAsync(model, null);
		return View(model);
	}

	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Edit(int id)
	{
		OCarousel oCarousel = await _carouselService.GetCarouselByIdAsync(id);
		if (oCarousel == null || oCarousel.Deleted)
		{
			return RedirectToAction("List");
		}
		return View(await _carouselModelFactory.PrepareOCarouselModelAsync(null, oCarousel));
	}

	[EditAccess(false)]
	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Edit(OCarouselModel model, bool continueEditing)
	{
		OCarousel carousel = await _carouselService.GetCarouselByIdAsync(model.Id);
		if (carousel == null || carousel.Deleted)
		{
			return RedirectToAction("List");
		}
		if (base.ModelState.IsValid)
		{
			carousel = model.ToEntity(carousel);
			carousel.UpdatedOnUtc = DateTime.UtcNow;
			await _carouselService.UpdateCarouselAsync(carousel);
			await UpdateLocalesAsync(carousel, model);
			await SaveStoreMappingsAsync(carousel, model);
			await _carouselService.UpdateCarouselAsync(carousel);
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Updated"));
			return continueEditing ? RedirectToAction("Edit", new
			{
				id = model.Id
			}) : RedirectToAction("List");
		}
		model = await _carouselModelFactory.PrepareOCarouselModelAsync(model, carousel);
		return View(model);
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Delete(OCarouselModel model)
	{
		OCarousel oCarousel = await _carouselService.GetCarouselByIdAsync(model.Id);
		if (oCarousel == null || oCarousel.Deleted)
		{
			return RedirectToAction("List");
		}
		await _carouselService.DeleteCarouselAsync(oCarousel);
		INotificationService notificationService = _notificationService;
		notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Deleted"));
		return RedirectToAction("List");
	}

	[HttpPost]
	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> OCarouselItemList(OCarouselItemSearchModel searchModel)
	{
		OCarousel oCarousel = await _carouselService.GetCarouselByIdAsync(searchModel.OCarouselId);
		if (oCarousel == null || oCarousel.Deleted)
		{
			return new NullJsonResult();
		}
		return Json(await _carouselModelFactory.PrepareOCarouselItemListModelAsync(searchModel, oCarousel));
	}

	[EditAccessAjax(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> OCarouselItemEdit(OCarouselItemModel model)
	{
		OCarousel oCarousel = await _carouselService.GetCarouselByIdAsync(model.OCarouselId);
		if (oCarousel == null || oCarousel.Deleted)
		{
			return new NullJsonResult();
		}
		OCarouselItem oCarouselItem = (await _carouselService.GetOCarouselItemsByOCarouselIdAsync(oCarousel.Id)).FirstOrDefault((OCarouselItem x) => x.Id == model.Id) ?? throw new ArgumentException("No carousel item found with the specified id", "Id");
		oCarouselItem.DisplayOrder = model.DisplayOrder;
		await _carouselService.UpdateOCarouselItemAsync(oCarouselItem);
		return new NullJsonResult();
	}

	[EditAccessAjax(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> OCarouselItemDelete(int ocarouselId, int id)
	{
		OCarousel oCarousel = await _carouselService.GetCarouselByIdAsync(ocarouselId);
		if (oCarousel == null || oCarousel.Deleted)
		{
			return new NullJsonResult();
		}
		OCarouselItem carouselItem = (await _carouselService.GetOCarouselItemsByOCarouselIdAsync(oCarousel.Id)).FirstOrDefault((OCarouselItem x) => x.Id == id) ?? throw new ArgumentException("No carousel item found with the specified id", "id");
		await _carouselService.DeleteOCarouselItemAsync(carouselItem);
		return new NullJsonResult();
	}

	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ProductAddPopup(int ocarouselId)
	{
		if (((await _carouselService.GetCarouselByIdAsync(ocarouselId)) ?? throw new ArgumentException("No carousel found with the specified id")).Deleted)
		{
			throw new ArgumentException("No carousel found with the specified id");
		}
		return View(await _carouselModelFactory.PrepareAddProductToOCarouselSearchModelAsync(new AddProductToCarouselSearchModel()));
	}

	[HttpPost]
	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ProductAddPopupList(AddProductToCarouselSearchModel searchModel)
	{
		return Json(await _carouselModelFactory.PrepareAddProductToOCarouselListModelAsync(searchModel));
	}

	[EditAccess(false)]
	[HttpPost]
	[FormValueRequired(new string[] { "save" })]
	[CheckPermission("ManageNopStationOCarousels", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ProductAddPopup(AddProductToCarouselModel model)
	{
		if (((await _carouselService.GetCarouselByIdAsync(model.OCarouselId)) ?? throw new ArgumentException("No carousel found with the specified id")).Deleted)
		{
			throw new ArgumentException("No carousel found with the specified id");
		}
		IPagedList<OCarouselItem> carouselItems = (await _carouselService.GetOCarouselItemsByOCarouselIdAsync(model.OCarouselId)) ?? throw new ArgumentException("No carousel item found with the specified id", "OCarouselId");
		IList<Product> list = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
		if (list.Count > 0)
		{
			foreach (Product product in list)
			{
				if (!carouselItems.Any((OCarouselItem x) => x.ProductId == product.Id))
				{
					await _carouselService.InsertOCarouselItemAsync(new OCarouselItem
					{
						DisplayOrder = 0,
						OCarouselId = model.OCarouselId,
						ProductId = product.Id
					});
				}
			}
		}
		base.ViewBag.RefreshPage = true;
		return View(new AddProductToCarouselSearchModel());
	}
}
