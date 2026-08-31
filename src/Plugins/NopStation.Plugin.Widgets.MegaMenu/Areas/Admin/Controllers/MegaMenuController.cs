using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Controllers;

public class MegaMenuController : BaseAdminController
{
	private readonly IStoreContext _storeContext;

	private readonly IBaseAdminModelFactory _baseAdminModelFactory;

	private readonly ILocalizationService _localizationService;

	private readonly INotificationService _notificationService;

	private readonly IManufacturerService _manufacturerService;

	private readonly IPermissionService _permissionService;

	private readonly MegaMenuSettings _megaMenuSettings;

	private readonly ICategoryService _categoryService;

	private readonly ISettingService _settingService;

	public MegaMenuController(IStoreContext storeContext, IBaseAdminModelFactory baseAdminModelFactory, ILocalizationService localizationService, INotificationService notificationService, IManufacturerService manufacturerService, IPermissionService permissionService, MegaMenuSettings megaMenuSettings, ICategoryService categoryService, ISettingService settingService)
	{
		_storeContext = storeContext;
		_baseAdminModelFactory = baseAdminModelFactory;
		_localizationService = localizationService;
		_notificationService = notificationService;
		_manufacturerService = manufacturerService;
		_permissionService = permissionService;
		_megaMenuSettings = megaMenuSettings;
		_categoryService = categoryService;
		_settingService = settingService;
	}

	public async Task<IActionResult> Configure()
	{
		if (!(await _permissionService.AuthorizeAsync("ManageNopStationMegaMenu")))
		{
			return AccessDeniedView();
		}
		int storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
		MegaMenuSettings megaMenuSettings = await _settingService.LoadSettingAsync<MegaMenuSettings>(storeId);
		ConfigurationModel model = megaMenuSettings.ToSettingsModel<ConfigurationModel>();
		await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories, withSpecialDefaultItem: false);
		await _baseAdminModelFactory.PrepareManufacturersAsync(model.AvailableManufacturers, withSpecialDefaultItem: false);
		if (!string.IsNullOrWhiteSpace(_megaMenuSettings.SelectedManufacturerIds))
		{
			model.SelectedManufacturerIds = _megaMenuSettings.SelectedManufacturerIds.Split(',').Select(int.Parse).ToList();
		}
		if (!string.IsNullOrWhiteSpace(_megaMenuSettings.SelectedCategoryIds))
		{
			model.SelectedCategoryIds = _megaMenuSettings.SelectedCategoryIds.Split(',').Select(int.Parse).ToList();
		}
		model.ActiveStoreScopeConfiguration = storeId;
		if (storeId <= 0)
		{
			return View(model);
		}
		ConfigurationModel configurationModel = model;
		configurationModel.EnableMegaMenu_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.EnableMegaMenu, storeId);
		configurationModel = model;
		configurationModel.HideManufacturers_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.HideManufacturers, storeId);
		configurationModel = model;
		configurationModel.MaxCategoryLevelsToShow_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.MaxCategoryLevelsToShow, storeId);
		configurationModel = model;
		configurationModel.SelectedCategoryIds_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.SelectedCategoryIds, storeId);
		configurationModel = model;
		configurationModel.SelectedManufacturerIds_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.SelectedManufacturerIds, storeId);
		configurationModel = model;
		configurationModel.ShowCategoryPicture_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowCategoryPicture, storeId);
		configurationModel = model;
		configurationModel.ShowMainCategoryPictureRight_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowMainCategoryPictureRight, storeId);
		configurationModel = model;
		configurationModel.ShowNumberOfCategoryProductsIncludeSubcategories_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowNumberOfCategoryProductsIncludeSubcategories, storeId);
		configurationModel = model;
		configurationModel.ShowNumberofCategoryProducts_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowNumberOfCategoryProducts, storeId);
		configurationModel = model;
		configurationModel.ShowManufacturerPicture_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowManufacturerPicture, storeId);
		configurationModel = model;
		configurationModel.ShowSubcategoryPicture_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowSubcategoryPicture, storeId);
		configurationModel = model;
		configurationModel.DefaultCategoryIconId_OverrideForStore = await _settingService.SettingExistsAsync(megaMenuSettings, (MegaMenuSettings x) => x.DefaultCategoryIconId, storeId);
		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		if (!(await _permissionService.AuthorizeAsync("ManageNopStationMegaMenu")))
		{
			return AccessDeniedView();
		}
		int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
		MegaMenuSettings megaMenuSettings = model.ToSettings(await _settingService.LoadSettingAsync<MegaMenuSettings>(storeScope));
		megaMenuSettings.SelectedCategoryIds = string.Join(",", model.SelectedCategoryIds);
		megaMenuSettings.SelectedManufacturerIds = string.Join(",", model.SelectedManufacturerIds);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.EnableMegaMenu, model.EnableMegaMenu_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.HideManufacturers, model.HideManufacturers_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.MaxCategoryLevelsToShow, model.MaxCategoryLevelsToShow_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.SelectedCategoryIds, model.SelectedCategoryIds_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.SelectedManufacturerIds, model.SelectedManufacturerIds_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowCategoryPicture, model.ShowCategoryPicture_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowNumberOfCategoryProductsIncludeSubcategories, model.ShowNumberOfCategoryProductsIncludeSubcategories_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowNumberOfCategoryProducts, model.ShowNumberofCategoryProducts_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowManufacturerPicture, model.ShowManufacturerPicture_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowSubcategoryPicture, model.ShowSubcategoryPicture_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.ShowMainCategoryPictureRight, model.ShowMainCategoryPictureRight_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(megaMenuSettings, (MegaMenuSettings x) => x.DefaultCategoryIconId, model.DefaultCategoryIconId_OverrideForStore, storeScope, clearCache: false);
		await _settingService.ClearCacheAsync();
		INotificationService notificationService = _notificationService;
		notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.Configuration.Updated"));
		return RedirectToAction("Configure");
	}
}
