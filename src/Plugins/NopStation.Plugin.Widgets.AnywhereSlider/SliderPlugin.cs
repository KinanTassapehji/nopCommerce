using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.AnywhereSlider.Components;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;
using NopStation.Plugin.Widgets.AnywhereSlider.Helpers;
using NopStation.Plugin.Widgets.AnywhereSlider.Services;

namespace NopStation.Plugin.Widgets.AnywhereSlider;

public class SliderPlugin : BasePlugin, IWidgetPlugin, IPlugin, INopStationPlugin
{
	private readonly IWebHelper _webHelper;

	private readonly ILocalizationService _localizationService;

	private readonly ISettingService _settingService;

	private readonly ISliderService _sliderService;

	private readonly IPictureService _pictureService;

	private readonly INopFileProvider _fileProvider;

	public bool HideInWidgetList => false;

	public SliderPlugin(IWebHelper webHelper, ILocalizationService localizationService, ISettingService settingService, ISliderService sliderService, IPictureService pictureService, INopFileProvider fileProvider)
	{
		_webHelper = webHelper;
		_localizationService = localizationService;
		_settingService = settingService;
		_sliderService = sliderService;
		_pictureService = pictureService;
		_fileProvider = fileProvider;
	}

	protected async Task CreateSampleDataAsync()
	{
		SliderSettings settings = new SliderSettings
		{
			EnableSlider = true
		};
		await _settingService.SaveSettingAsync(settings);
		Slider slider = new Slider
		{
			Active = true,
			AutoPlay = true,
			AutoPlayTimeout = 3000,
			AutoPlayHoverPause = true,
			CreatedOnUtc = DateTime.UtcNow,
			Name = "Home page top",
			Loop = true,
			UpdatedOnUtc = DateTime.UtcNow,
			Nav = true,
			DisplayOrder = 0,
			StartPosition = 0,
			WidgetZoneId = 5
		};
		await _sliderService.InsertSliderAsync(slider);
		string sampleImagesPath = _fileProvider.MapPath("~/Plugins/NopStation.Plugin.Widgets.AnywhereSlider/Content/sample/");
		SliderItem sliderItem = new SliderItem();
		SliderItem sliderItem2 = sliderItem;
		IPictureService pictureService = _pictureService;
		sliderItem2.PictureId = (await pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-1.jpg")), MimeTypes.ImageJpeg, "slider-1")).Id;
		SliderItem sliderItem3 = sliderItem;
		pictureService = _pictureService;
		sliderItem3.MobilePictureId = (await pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-1-mobile.jpg")), MimeTypes.ImageJpeg, "slider-1")).Id;
		sliderItem.Title = "Liquid for Chicken";
		sliderItem.ShortDescription = "The Best General Tso's Chicken";
		sliderItem.SliderId = slider.Id;
		await _sliderService.InsertSliderItemAsync(sliderItem);
		sliderItem2 = new SliderItem();
		sliderItem = sliderItem2;
		pictureService = _pictureService;
		sliderItem.PictureId = (await pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-2.jpg")), MimeTypes.ImageJpeg, "slider-2")).Id;
		sliderItem3 = sliderItem2;
		pictureService = _pictureService;
		sliderItem3.MobilePictureId = (await pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-2-mobile.jpg")), MimeTypes.ImageJpeg, "slider-2")).Id;
		sliderItem2.Title = "Pressure Cooker";
		sliderItem2.ShortDescription = "Ribollita Into a Weeknight Meal";
		sliderItem2.SliderId = slider.Id;
		await _sliderService.InsertSliderItemAsync(sliderItem2);
		sliderItem = new SliderItem();
		sliderItem2 = sliderItem;
		pictureService = _pictureService;
		sliderItem2.PictureId = (await pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-3.jpg")), MimeTypes.ImageJpeg, "slider-3")).Id;
		sliderItem3 = sliderItem;
		pictureService = _pictureService;
		sliderItem3.MobilePictureId = (await pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-3-mobile.jpg")), MimeTypes.ImageJpeg, "slider-3")).Id;
		sliderItem.Title = "Ingredients";
		sliderItem.ShortDescription = "The Best General Tso's Chicken";
		sliderItem.SliderId = slider.Id;
		await _sliderService.InsertSliderItemAsync(sliderItem);
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/AnywhereSlider/Configure";
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		if (widgetZone == PublicWidgetZones.Footer)
		{
			return typeof(AnywhereSliderFooterViewComponent);
		}
		return typeof(AnywhereSliderViewComponent);
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		List<string> customWidgetZones = SliderHelper.GetCustomWidgetZones();
		customWidgetZones.Add(PublicWidgetZones.Footer);
		return Task.FromResult((IList<string>)customWidgetZones);
	}

	public override async Task InstallAsync()
	{
		await CreateSampleDataAsync();
		await this.InstallPluginAsync();
		await base.InstallAsync();
	}

	public override async Task UninstallAsync()
	{
		await this.UninstallPluginAsync();
		await base.UninstallAsync();
	}

	public override async Task UpdateAsync(string currentVersion, string targetVersion)
	{
		await _localizationService.AddOrUpdateLocaleResourceAsync(GetPluginResources());
		await base.UpdateAsync(currentVersion, targetVersion);
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["Admin.NopStation.AnywhereSlider.Sliders.List.SearchActive.Active"] = "Active",
			["Admin.NopStation.AnywhereSlider.Sliders.List.SearchActive.Inactive"] = "Inactive",
			["Admin.NopStation.AnywhereSlider.Menu.AnywhereSlider"] = "Anywhere slider",
			["Admin.NopStation.AnywhereSlider.Menu.Configuration"] = "Configuration",
			["Admin.NopStation.AnywhereSlider.Menu.Sliders"] = "Sliders",
			["Admin.NopStation.AnywhereSlider.Configuration"] = "Slider settings",
			["Admin.NopStation.AnywhereSlider.Tab.Info"] = "Info",
			["Admin.NopStation.AnywhereSlider.Tab.Properties"] = "Properties",
			["Admin.NopStation.AnywhereSlider.Tab.SliderItems"] = "Slider items",
			["Admin.NopStation.AnywhereSlider.SliderList"] = "Sliders",
			["Admin.NopStation.AnywhereSlider.EditDetails"] = "Edit slider details",
			["Admin.NopStation.AnywhereSlider.BackToList"] = "back to slider list",
			["Admin.NopStation.AnywhereSlider.AddNew"] = "Add new slider",
			["Admin.NopStation.AnywhereSlider.SliderItems.SaveBeforeEdit"] = "You need to save the slider before you can add items for this slider page.",
			["Admin.NopStation.AnywhereSlider.SliderItems.AddNew"] = "Add new item",
			["Admin.NopStation.AnywhereSlider.SliderItems.Pictures.Alert.PictureAdd"] = "Failed to add product picture.",
			["Admin.NopStation.AnywhereSlider.Configuration.Fields.EnableSlider"] = "Enable slider",
			["Admin.NopStation.AnywhereSlider.Configuration.Fields.EnableSlider.Hint"] = "Check to enable slider for your store.",
			["Admin.NopStation.AnywhereSlider.Configuration.Updated"] = "Slider configuration updated successfully.",
			["Admin.NopStation.AnywhereSlider.Sliders.Created"] = "Slider has been created successfully.",
			["Admin.NopStation.AnywhereSlider.Sliders.Updated"] = "Slider has been updated successfully.",
			["Admin.NopStation.AnywhereSlider.Sliders.Deleted"] = "Slider has been deleted successfully.",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.DisplayOrder"] = "Display order",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.Picture"] = "Picture",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.MobilePicture"] = "Mobile picture",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.ImageAltText"] = "Alt",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.Title"] = "Title",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.ShortDescription"] = "Short description",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.Link"] = "Link",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.DisplayOrder.Hint"] = "The display order for this slider item. 1 represents the top of the list.",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.Picture.Hint"] = "Picture of this slider item.",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.MobilePicture.Hint"] = "Mobile view picture of this slider item.",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.ImageAltText.Hint"] = "Override \"alt\" attribute for \"img\" HTML element.",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.Title.Hint"] = "Override \"title\" attribute for \"img\" HTML element.",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.Link.Hint"] = "Custom link for slider item picture.",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.ShortDescription.Hint"] = "Short description for this slider item.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Name"] = "Name",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Name.Hint"] = "The slider name.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Title"] = "Title",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Title.Hint"] = "The slider title.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.DisplayTitle"] = "Display title",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.DisplayTitle.Hint"] = "Determines whether title should be displayed on public site (depends on theme design).",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Active"] = "Active",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Active.Hint"] = "Determines whether this slider is active (visible on public store).",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.WidgetZone"] = "Widget zone",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.WidgetZone.Hint"] = "The widget zone where this slider will be displayed.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Picture"] = "Picture",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Picture.Hint"] = "The slider picture.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.CustomUrl"] = "Custom url",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.CustomUrl.Hint"] = "The slider custom url.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlay"] = "Auto play",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlay.Hint"] = "Check to enable auto play.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.CustomCssClass"] = "Custom css class",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.CustomCssClass.Hint"] = "Enter the custom CSS class to be applied.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.DisplayOrder"] = "Display order",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.DisplayOrder.Hint"] = "Display order of the slider. 1 represents the top of the list.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Loop"] = "Loop",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Loop.Hint"] = "heck to enable loop.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Margin"] = "Margin",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Margin.Hint"] = "It's margin-right (px) on item.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.StartPosition"] = "Start position",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.StartPosition.Hint"] = "Starting position.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Center"] = "Center",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Center.Hint"] = "Check to center item. It works well with even and odd number of items.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Nav"] = "NAV",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Nav.Hint"] = "Check to enable next/prev buttons.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.LazyLoad"] = "Lazy load",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.LazyLoad.Hint"] = "Check to enable lazy load.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.LazyLoadEager"] = "Lazy load eager",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.LazyLoadEager.Hint"] = "Specify how many items you want to pre-load images to the right (and left when loop is enabled).",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlayTimeout"] = "Auto play timeout",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlayTimeout.Hint"] = "It's autoplay interval timeout. (e.g 5000)",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlayHoverPause"] = "Auto play hover pause",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlayHoverPause.Hint"] = "Check to enable pause on mouse hover.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.AnimateOut"] = "Animate out",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.AnimateOut.Hint"] = "Animate out.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.AnimateIn"] = "Animate in",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.AnimateIn.Hint"] = "Animate in.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.CreatedOn"] = "Created on",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.CreatedOn.Hint"] = "The create date of this slider.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.UpdatedOn"] = "Updated on",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.UpdatedOn.Hint"] = "The last update date of this slider.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.SelectedStoreIds"] = "Limited to stores",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.SelectedStoreIds.Hint"] = "Option to limit this slider to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.Name.Required"] = "The name field is required.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.BackGroundPicture.Required"] = "The background picture is required.",
			["Admin.NopStation.AnywhereSlider.Sliders.List.SearchWidgetZones"] = "Widget zones",
			["Admin.NopStation.AnywhereSlider.Sliders.List.SearchWidgetZones.Hint"] = "The search widget zones.",
			["Admin.NopStation.AnywhereSlider.Sliders.List.SearchStore"] = "Store",
			["Admin.NopStation.AnywhereSlider.Sliders.List.SearchStore.Hint"] = "The search store.",
			["Admin.NopStation.AnywhereSlider.Sliders.List.SearchActive"] = "Active",
			["Admin.NopStation.AnywhereSlider.Sliders.List.SearchActive.Hint"] = "The search active.",
			["Admin.NopStation.AnywhereSlider.SliderItems.EditDetails"] = "Edit details",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.Title.Required"] = "Title is required.",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.Picture.Required"] = "Picture is required.",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.MobilePicture.Required"] = "Mobile picture is required.",
			["Admin.NopStation.AnywhereSlider.SliderItems.Pictures.Alert.AddNew"] = "Upload picture first.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.ShowBackgroundPicture"] = "Show background picture",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.ShowBackgroundPicture.Hint"] = "Determines whether to show background picture or not.",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.BackgroundPicture"] = "Background picture",
			["Admin.NopStation.AnywhereSlider.Sliders.Fields.BackgroundPicture.Hint"] = "Background picture for this slider.",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.ShopNowLink"] = "ShopNow Link",
			["Admin.NopStation.AnywhereSlider.SliderItems.Fields.ShopNowLink.Hint"] = "Your ShopNow Link",
			["NopStation.AnywhereSlider.ShopNow"] = "Shop Now",
			["NopStation.AnywhereSlider.LoadingFailed"] = "Failed to load slider content."
		};
	}
}
