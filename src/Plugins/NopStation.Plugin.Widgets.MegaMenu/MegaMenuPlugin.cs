using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Widgets.MegaMenu;

public class MegaMenuPlugin : BasePlugin, INopStationPlugin, IPlugin, IMiscPlugin
{
	private readonly IWebHelper _webHelper;

	private readonly ILocalizationService _localizationService;

	private readonly IPermissionService _permissionService;

	private readonly INopFileProvider _fileProvider;

	private readonly IPictureService _pictureService;

	private readonly ISettingService _settingService;

	public MegaMenuPlugin(IWebHelper webHelper, ILocalizationService localizationService, IPermissionService permissionService, INopFileProvider fileProvider, IPictureService pictureService, ISettingService settingService)
	{
		_webHelper = webHelper;
		_localizationService = localizationService;
		_permissionService = permissionService;
		_fileProvider = fileProvider;
		_pictureService = pictureService;
		_settingService = settingService;
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/MegaMenu/Configure";
	}

	public override async Task InstallAsync()
	{
		string text = _fileProvider.MapPath("~/Plugins/NopStation.Plugin.Widgets.MegaMenu/Install/");
		IPictureService pictureService = _pictureService;
		int id = (await pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(text, "nop-station.png")), MimeTypes.ImagePng, "nop-station")).Id;
		MegaMenuSettings settings = new MegaMenuSettings
		{
			EnableMegaMenu = true,
			MaxCategoryLevelsToShow = 4,
			ShowNumberOfCategoryProducts = true,
			ShowNumberOfCategoryProductsIncludeSubcategories = true,
			DefaultCategoryIconId = id
		};
		await _settingService.SaveSettingAsync(settings);
		await this.InstallPluginAsync();
		await base.InstallAsync();
	}

	public override async Task UninstallAsync()
	{
		await this.UninstallPluginAsync(new MegaMenuPermissionConfigManager());
		await base.UninstallAsync();
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["Admin.NopStation.MegaMenu.Menu.MegaMenu"] = "Mega menu",
			["Admin.NopStation.MegaMenu.Menu.Configuration"] = "Configuration",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowMainCategoryPictureRight"] = "Enable Left panel image",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowMainCategoryPictureRight.Hint"] = "Enable Left panel image",
			["Admin.NopStation.MegaMenu.Configuration.Fields.EnableMegaMenu"] = "Enable mega menu",
			["Admin.NopStation.MegaMenu.Configuration.Fields.EnableMegaMenu.Hint"] = "Check to enable mega menu. Restart application after changing value of this property.",
			["Admin.NopStation.MegaMenu.Configuration.Fields.MaxCategoryLevelsToShow"] = "Max category level",
			["Admin.NopStation.MegaMenu.Configuration.Fields.MaxCategoryLevelsToShow.Hint"] = "Maximum category level to be displayed on top menu.",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowNumberOfCategoryProducts"] = "Show number of category products",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowNumberOfCategoryProducts.Hint"] = "Determines whether number of category products to be displayed on top menu or not.",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowNumberOfCategoryProductsIncludeSubcategories"] = "Include sub-category products",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowNumberOfCategoryProductsIncludeSubcategories.Hint"] = "Show category product number including sub-categories.",
			["Admin.NopStation.MegaMenu.Configuration.Fields.SelectedCategoryIds"] = "Selected categories",
			["Admin.NopStation.MegaMenu.Configuration.Fields.SelectedCategoryIds.Hint"] = "Selected categories to be displayed on top menu.",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowCategoryPicture"] = "Show category picture",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowCategoryPicture.Hint"] = "Show category picture on top menu.",
			["Admin.NopStation.MegaMenu.Configuration.Fields.DefaultCategoryIconId"] = "Default category icon",
			["Admin.NopStation.MegaMenu.Configuration.Fields.DefaultCategoryIconId.Hint"] = "The default category icon to show on mega menu",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowSubcategoryPicture"] = "Show sub-category picture",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowSubcategoryPicture.Hint"] = "Show sub-category picture on top menu.",
			["Admin.NopStation.MegaMenu.Configuration.Fields.HideManufacturers"] = "Hide manufacturers",
			["Admin.NopStation.MegaMenu.Configuration.Fields.HideManufacturers.Hint"] = "Hide manufacturers from top menu.",
			["Admin.NopStation.MegaMenu.Configuration.Fields.SelectedManufacturerIds"] = "Selected manufacturers",
			["Admin.NopStation.MegaMenu.Configuration.Fields.SelectedManufacturerIds.Hint"] = "Selected manufacturers to be displayed on top menu.",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowManufacturerPicture"] = "Show manufacturer picture",
			["Admin.NopStation.MegaMenu.Configuration.Fields.ShowManufacturerPicture.Hint"] = "Show manufacturer picture on top menu.",
			["Admin.NopStation.MegaMenu.Configuration.Updated"] = "Mega menu configuration has been updated successfully.",
			["Admin.NopStation.MegaMenu.Configuration"] = "Mega menu settings",
			["NopStation.MegaMenu.Public.Categories"] = "Categories",
			["NopStation.MegaMenu.Public.Manufacturers"] = "Manufacturers",
			["NopStation.MegaMenu.Public.AllManufacturers"] = "All Manufacturers",
			["Admin.NopStation.MegaMenu.CategoryIcons"] = "Category Icons",
			["Admin.NopStation.MegaMenu.CategoryIcons.Fields.Category"] = "Category",
			["Admin.NopStation.MegaMenu.CategoryIcons.Fields.Picture"] = "Picture",
			["Admin.NopStation.MegaMenu.CategoryIcons.List.SearchCategoryName"] = "Search Category Name",
			["Admin.NopStation.MegaMenu.CategoryIcons.List.SearchStore"] = "Search Store",
			["Admin.NopStation.MegaMenu.CategoryIcons.Fields.Picture.Required"] = "Picture is required",
			["Admin.NopStation.MegaMenu.CategoryIcons.Fields.Category.Required"] = "Category is required",
			["Admin.NopStation.MegaMenu.CategoryIcons.AddNew"] = "Add New",
			["Admin.NopStation.MegaMenu.CategoryIcons.BackToList"] = "Back To List",
			["Admin.NopStation.MegaMenu.CategoryIcons.EditDetails"] = "Edit Details",
			["Admin.NopStation.MegaMenu.CategoryIcons.List"] = "List"
		};
	}
}
