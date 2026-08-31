using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;
using NopStation.Plugin.Widgets.AnywhereSlider.Services;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Controllers;

public class AnywhereSliderController : NopStationAdminController
{
	private readonly ISettingHelper<SliderSettings, ConfigurationModel> _settingHelper;

	private readonly IStoreContext _storeContext;

	private readonly ILocalizedEntityService _localizedEntityService;

	private readonly ILocalizationService _localizationService;

	private readonly INotificationService _notificationService;

	private readonly IStoreMappingService _storeMappingService;

	private readonly ISliderModelFactory _sliderModelFactory;

	private readonly ISliderService _sliderService;

	private readonly IStoreService _storeService;

	public AnywhereSliderController(ISettingHelper<SliderSettings, ConfigurationModel> settingHelper, IStoreContext storeContext, ILocalizedEntityService localizedEntityService, ILocalizationService localizationService, INotificationService notificationService, IStoreMappingService storeMappingService, ISliderModelFactory sliderModelFactory, ISliderService sliderService, IStoreService storeService)
	{
		_settingHelper = settingHelper;
		_storeContext = storeContext;
		_localizedEntityService = localizedEntityService;
		_localizationService = localizationService;
		_notificationService = notificationService;
		_storeMappingService = storeMappingService;
		_sliderModelFactory = sliderModelFactory;
		_sliderService = sliderService;
		_storeService = storeService;
	}

	protected virtual async Task SaveStoreMappingsAsync(Slider slider, SliderModel model)
	{
		slider.LimitedToStores = model.SelectedStoreIds.Any();
		IList<StoreMapping> existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(slider);
		foreach (Store store in await _storeService.GetAllStoresAsync())
		{
			if (model.SelectedStoreIds.Contains(store.Id))
			{
				if (existingStoreMappings.All((StoreMapping sm) => sm.StoreId != store.Id))
				{
					await _storeMappingService.InsertStoreMappingAsync(slider, store.Id);
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

	protected virtual async Task UpdateLocalesAsync(Slider slider, SliderModel model)
	{
		foreach (SliderLocalizedModel locale in model.Locales)
		{
			await _localizedEntityService.SaveLocalizedValueAsync(slider, (Slider x) => x.Name, locale.Name, locale.LanguageId);
		}
	}

	protected virtual async Task UpdateLocalesAsync(SliderItem sliderItem, SliderItemModel model)
	{
		foreach (SliderItemLocalizedModel localized in model.Locales)
		{
			await _localizedEntityService.SaveLocalizedValueAsync(sliderItem, (SliderItem x) => x.Title, localized.SliderItemTitle, localized.LanguageId);
			await _localizedEntityService.SaveLocalizedValueAsync(sliderItem, (SliderItem x) => x.ShortDescription, localized.ShortDescription, localized.LanguageId);
			await _localizedEntityService.SaveLocalizedValueAsync(sliderItem, (SliderItem x) => x.Link, localized.Link, localized.LanguageId);
			await _localizedEntityService.SaveLocalizedValueAsync(sliderItem, (SliderItem x) => x.ShopNowLink, localized.ShopNowLink, localized.LanguageId);
			await _localizedEntityService.SaveLocalizedValueAsync(sliderItem, (SliderItem x) => x.ImageAltText, localized.ImageAltText, localized.LanguageId);
		}
	}

	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		return View(await _settingHelper.PrepareConfigurationModelAsync(null));
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		await _settingHelper.SaveConfigurationModelAsync(model, null, true);
		return RedirectToAction("Configure");
	}

	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> List()
	{
		return View(await _sliderModelFactory.PrepareSliderSearchModelAsync(new SliderSearchModel()));
	}

	[HttpPost]
	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> List(SliderSearchModel searchModel)
	{
		return Json(await _sliderModelFactory.PrepareSliderListModelAsync(searchModel));
	}

	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Create()
	{
		return View(await _sliderModelFactory.PrepareSliderModelAsync(new SliderModel(), null));
	}

	[EditAccess(false)]
	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Create(SliderModel model, bool continueEditing)
	{
		if (base.ModelState.IsValid)
		{
			Slider slider = model.ToEntity<Slider>();
			slider.CreatedOnUtc = DateTime.UtcNow;
			slider.UpdatedOnUtc = DateTime.UtcNow;
			await _sliderService.InsertSliderAsync(slider);
			await UpdateLocalesAsync(slider, model);
			await SaveStoreMappingsAsync(slider, model);
			await _sliderService.UpdateSliderAsync(slider);
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Sliders.Created"));
			return continueEditing ? RedirectToAction("Edit", new
			{
				id = slider.Id
			}) : RedirectToAction("List");
		}
		model = await _sliderModelFactory.PrepareSliderModelAsync(model, null);
		return View(model);
	}

	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Edit(int id)
	{
		Slider slider = await _sliderService.GetSliderByIdAsync(id);
		if (slider == null || slider.Deleted)
		{
			return RedirectToAction("List");
		}
		return View(await _sliderModelFactory.PrepareSliderModelAsync(null, slider));
	}

	[EditAccess(false)]
	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Edit(SliderModel model, bool continueEditing)
	{
		Slider slider = await _sliderService.GetSliderByIdAsync(model.Id);
		if (slider == null || slider.Deleted)
		{
			return RedirectToAction("List");
		}
		if (base.ModelState.IsValid)
		{
			slider = model.ToEntity(slider);
			slider.UpdatedOnUtc = DateTime.UtcNow;
			await _sliderService.UpdateSliderAsync(slider);
			await UpdateLocalesAsync(slider, model);
			await SaveStoreMappingsAsync(slider, model);
			await _sliderService.UpdateSliderAsync(slider);
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Sliders.Updated"));
			return continueEditing ? RedirectToAction("Edit", new
			{
				id = model.Id
			}) : RedirectToAction("List");
		}
		model = await _sliderModelFactory.PrepareSliderModelAsync(model, slider);
		return View(model);
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Delete(int id)
	{
		Slider slider = await _sliderService.GetSliderByIdAsync(id);
		if (slider == null || slider.Deleted)
		{
			return RedirectToAction("List");
		}
		await _sliderService.DeleteSliderAsync(slider);
		INotificationService notificationService = _notificationService;
		notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Sliders.Deleted"));
		return RedirectToAction("List");
	}

	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> SliderItemCreatePopup(int sliderId)
	{
		if (sliderId == 0)
		{
			return Content("");
		}
		Slider slider = (await _sliderService.GetSliderByIdAsync(sliderId)) ?? throw new ArgumentException("No slider found with the specified id", "sliderId");
		return View(await _sliderModelFactory.PrepareSliderItemModelAsync(new SliderItemModel(), slider, null));
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> SliderItemCreatePopup(SliderItemModel model)
	{
		Slider slider = (await _sliderService.GetSliderByIdAsync(model.SliderId)) ?? throw new ArgumentException("No slider found with the specified id");
		if (base.ModelState.IsValid)
		{
			SliderItem sliderItem = model.ToEntity<SliderItem>();
			await _sliderService.InsertSliderItemAsync(sliderItem);
			await UpdateLocalesAsync(sliderItem, model);
			base.ViewBag.RefreshPage = true;
			return View(model);
		}
		model = await _sliderModelFactory.PrepareSliderItemModelAsync(model, slider, null);
		return View(model);
	}

	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> SliderItemEditPopup(int id)
	{
		SliderItem sliderItem = (await _sliderService.GetSliderItemByIdAsync(id)) ?? throw new ArgumentException("No slider item found with the specified id");
		Slider slider = (await _sliderService.GetSliderByIdAsync(sliderItem.SliderId)) ?? throw new ArgumentException("No slider found with the specified id");
		return View(await _sliderModelFactory.PrepareSliderItemModelAsync(null, slider, sliderItem));
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> SliderItemEditPopup(SliderItemModel model)
	{
		SliderItem sliderItem = (await _sliderService.GetSliderItemByIdAsync(model.Id)) ?? throw new ArgumentException("No slider item found with the specified id");
		Slider slider = (await _sliderService.GetSliderByIdAsync(sliderItem.SliderId)) ?? throw new ArgumentException("No slider found with the specified id");
		if (base.ModelState.IsValid)
		{
			sliderItem = model.ToEntity(sliderItem);
			sliderItem.Title = model.SliderItemTitle;
			await _sliderService.UpdateSliderItemAsync(sliderItem);
			await UpdateLocalesAsync(sliderItem, model);
			base.ViewBag.RefreshPage = true;
			return View(model);
		}
		model = await _sliderModelFactory.PrepareSliderItemModelAsync(model, slider, sliderItem, excludeProperties: true);
		return View(model);
	}

	[EditAccessAjax(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> SliderItemDelete(int id)
	{
		SliderItem sliderItem = (await _sliderService.GetSliderItemByIdAsync(id)) ?? throw new ArgumentException("No slider item found with the specified id");
		if ((await _sliderService.GetSliderByIdAsync(sliderItem.SliderId)).Deleted)
		{
			return new NullJsonResult();
		}
		await _sliderService.DeleteSliderItemAsync(sliderItem);
		return new NullJsonResult();
	}

	[HttpPost]
	[CheckPermission("ManageNopStationSliders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> SliderItemList(SliderItemSearchModel searchModel)
	{
		return Json(await _sliderModelFactory.PrepareSliderItemListModelAsync(searchModel));
	}
}
