using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.OCarousels.Components;
using NopStation.Plugin.Widgets.OCarousels.Domains;
using NopStation.Plugin.Widgets.OCarousels.Helpers;
using NopStation.Plugin.Widgets.OCarousels.Services;

namespace NopStation.Plugin.Widgets.OCarousels;

public class OCarouselPlugin : BasePlugin, IWidgetPlugin, IPlugin, INopStationPlugin
{
	private readonly IWebHelper _webHelper;

	private readonly ISettingService _settingService;

	private readonly IOCarouselService _carouselService;

	private readonly ILocalizationService _localizationService;

	private readonly ILocalizedEntityService _localizedEntityService;

	private readonly ILanguageService _languageService;

	public bool HideInWidgetList => false;

	public OCarouselPlugin(IWebHelper webHelper, ISettingService settingService, IOCarouselService carouselService, ILocalizationService localizationService, ILocalizedEntityService localizedEntityService, ILanguageService languageService)
	{
		_webHelper = webHelper;
		_settingService = settingService;
		_carouselService = carouselService;
		_localizationService = localizationService;
		_localizedEntityService = localizedEntityService;
		_languageService = languageService;
	}

	private async Task CreateSampleDataAsync()
	{
		OCarouselSettings settings = new OCarouselSettings
		{
			EnableOCarousel = true
		};
		await _settingService.SaveSettingAsync(settings);
		OCarousel oCarousel = new OCarousel
		{
			Active = true,
			AutoPlay = true,
			AutoPlayHoverPause = true,
			AutoPlayTimeout = 3000,
			CreatedOnUtc = DateTime.UtcNow,
			DataSourceTypeEnum = DataSourceTypeEnum.HomePageCategories,
			DisplayTitle = true,
			Loop = true,
			LazyLoad = true,
			Name = "Featured Categories",
			Nav = true,
			UpdatedOnUtc = DateTime.UtcNow,
			NumberOfItemsToShow = 10,
			Title = "Featured Categories",
			WidgetZoneId = 2
		};
		await _carouselService.InsertCarouselAsync(oCarousel);
		OCarousel oCarousel2 = new OCarousel
		{
			Active = true,
			AutoPlay = true,
			AutoPlayHoverPause = true,
			AutoPlayTimeout = 3000,
			CreatedOnUtc = DateTime.UtcNow,
			DataSourceTypeEnum = DataSourceTypeEnum.NewProducts,
			DisplayTitle = true,
			Loop = true,
			LazyLoad = true,
			Name = "New Products",
			Nav = true,
			UpdatedOnUtc = DateTime.UtcNow,
			NumberOfItemsToShow = 10,
			Title = "New Products",
			WidgetZoneId = 3
		};
		await _carouselService.InsertCarouselAsync(oCarousel2);
		OCarousel oCarousel3 = new OCarousel
		{
			Active = true,
			AutoPlay = true,
			AutoPlayHoverPause = true,
			AutoPlayTimeout = 3000,
			CreatedOnUtc = DateTime.UtcNow,
			DataSourceTypeEnum = DataSourceTypeEnum.BestSellers,
			DisplayTitle = true,
			Loop = true,
			LazyLoad = true,
			Name = "Best Sellers",
			Nav = true,
			UpdatedOnUtc = DateTime.UtcNow,
			NumberOfItemsToShow = 10,
			Title = "Best Sellers",
			WidgetZoneId = 4
		};
		await _carouselService.InsertCarouselAsync(oCarousel3);
		OCarousel oCarousel4 = new OCarousel
		{
			Active = true,
			AutoPlay = true,
			AutoPlayHoverPause = true,
			AutoPlayTimeout = 3000,
			CreatedOnUtc = DateTime.UtcNow,
			DataSourceTypeEnum = DataSourceTypeEnum.Manufacturers,
			DisplayTitle = true,
			Loop = true,
			LazyLoad = true,
			Name = "Manufacturers",
			Nav = true,
			UpdatedOnUtc = DateTime.UtcNow,
			NumberOfItemsToShow = 10,
			Title = "Manufacturers",
			WidgetZoneId = 6
		};
		await _carouselService.InsertCarouselAsync(oCarousel4);
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/OCarousel/Configure";
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		if (widgetZone == PublicWidgetZones.Footer)
		{
			return typeof(OCarouselFooterViewComponent);
		}
		return typeof(OCarouselViewComponent);
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		List<string> customWidgetZones = OCarouselHelper.GetCustomWidgetZones();
		customWidgetZones.Add(PublicWidgetZones.Footer);
		return Task.FromResult((IList<string>)customWidgetZones);
	}

	//the sample carousels are seeded in English; their titles are entity data, so the ar-SA
	//language pack cannot carry them
	private async Task TranslateSampleCarouselTitlesAsync()
	{
		Language arabic = (await _languageService.GetAllLanguagesAsync(showHidden: true))
			.FirstOrDefault((Language language) => language.LanguageCulture == "ar-SA");
		if (arabic == null)
		{
			return;
		}
		Dictionary<string, string> titles = new Dictionary<string, string>
		{
			["Featured Categories"] = "فئات مميزة",
			["New Products"] = "منتجات جديدة",
			["Best Sellers"] = "الأكثر مبيعا",
			["Manufacturers"] = "الشركات"
		};
		foreach (OCarousel carousel in await _carouselService.GetAllCarouselsAsync())
		{
			if (carousel.Title != null && titles.TryGetValue(carousel.Title, out var title))
			{
				await _localizedEntityService.SaveLocalizedValueAsync(carousel, (OCarousel x) => x.Title, title, arabic.Id);
			}
		}
	}

	public override async Task InstallAsync()
	{
		await CreateSampleDataAsync();
		await TranslateSampleCarouselTitlesAsync();
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
		await TranslateSampleCarouselTitlesAsync();
		await base.UpdateAsync(currentVersion, targetVersion);
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["Admin.NopStation.OCarousels.OCarousels.List.SearchActive.Active"] = "Active",
			["Admin.NopStation.OCarousels.OCarousels.List.SearchActive.Inactive"] = "Inactive",
			["Admin.NopStation.OCarousels.Menu.OCarousel"] = "OCarousel",
			["Admin.NopStation.OCarousels.Menu.Configuration"] = "Configuration",
			["Admin.NopStation.OCarousels.Menu.Carousels"] = "Carousels",
			["Admin.NopStation.OCarousels.Configuration"] = "Carousel settings",
			["Admin.NopStation.OCarousels.Tab.Info"] = "Info",
			["Admin.NopStation.OCarousels.Tab.Properties"] = "Properties",
			["Admin.NopStation.OCarousels.Tab.OCarouselItems"] = "Carousel items",
			["Admin.NopStation.OCarousels.CarouselList"] = "Carousels",
			["Admin.NopStation.OCarousels.EditDetails"] = "Edit carousel details",
			["Admin.NopStation.OCarousels.BackToList"] = "back to carousel list",
			["Admin.NopStation.OCarousels.AddNew"] = "Add new carousel",
			["Admin.NopStation.OCarousels.OCarouselItems.AddNew"] = "Add new item",
			["Admin.NopStation.OCarousels.OCarouselItems.SaveBeforeEdit"] = "You need to save the carousel before you can add items for this carousel page.",
			["Admin.NopStation.OCarousels.Configuration.Fields.EnableOCarousel"] = "Enable carousel",
			["Admin.NopStation.OCarousels.Configuration.Fields.EnableOCarousel.Hint"] = "Check to enable carousel for your store.",
			["Admin.NopStation.OCarousels.Configuration.Fields.RequireOCarouselPicture"] = "Require carousel picture",
			["Admin.NopStation.OCarousels.Configuration.Fields.RequireOCarouselPicture.Hint"] = "Determines whether main picture is required for carousel (based on theme design).",
			["Admin.NopStation.OCarousels.Created"] = "Carousel has been created successfully.",
			["Admin.NopStation.OCarousels.Updated"] = "Carousel has been updated successfully.",
			["Admin.NopStation.OCarousels.Deleted"] = "Carousel has been deleted successfully.",
			["Admin.NopStation.OCarousels.OCarouselItems.Fields.Product"] = "Product",
			["Admin.NopStation.OCarousels.OCarouselItems.Fields.OCarousel"] = "Carousel",
			["Admin.NopStation.OCarousels.OCarouselItems.Fields.DisplayOrder"] = "Display order",
			["Admin.NopStation.OCarousels.OCarouselItems.Fields.Picture"] = "Picture",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Name"] = "Name",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Name.Hint"] = "The carousel name.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Title"] = "Title",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Title.Hint"] = "The carousel title.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.DisplayTitle"] = "Display title",
			["Admin.NopStation.OCarousels.OCarousels.Fields.DisplayTitle.Hint"] = "Determines whether title should be displayed on public site (depends on theme design).",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Active"] = "Active",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Active.Hint"] = "Determines whether this carousel is active (visible on public store).",
			["Admin.NopStation.OCarousels.OCarousels.Fields.WidgetZone"] = "Widget zone",
			["Admin.NopStation.OCarousels.OCarousels.Fields.WidgetZone.Hint"] = "The widget zone where this carousel will be displayed.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.DataSourceType"] = "Data source type",
			["Admin.NopStation.OCarousels.OCarousels.Fields.DataSourceType.Hint"] = "The data source for this carousel.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Picture"] = "Picture",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Picture.Hint"] = "The carousel picture.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.CustomUrl"] = "Custom url",
			["Admin.NopStation.OCarousels.OCarousels.Fields.CustomUrl.Hint"] = "The carousel custom url.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.NumberOfItemsToShow"] = "Number of items to show",
			["Admin.NopStation.OCarousels.OCarousels.Fields.NumberOfItemsToShow.Hint"] = "Specify the number of items to show for this carousel.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlay"] = "Auto play",
			["Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlay.Hint"] = "Check to enable auto play.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.CustomCssClass"] = "Custom css class",
			["Admin.NopStation.OCarousels.OCarousels.Fields.CustomCssClass.Hint"] = "Enter the custom CSS class to be applied.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.DisplayOrder"] = "Display order",
			["Admin.NopStation.OCarousels.OCarousels.Fields.DisplayOrder.Hint"] = "Display order of the carousel. 1 represents the top of the list.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Loop"] = "Loop",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Loop.Hint"] = "heck to enable loop.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.StartPosition"] = "Start position",
			["Admin.NopStation.OCarousels.OCarousels.Fields.StartPosition.Hint"] = "TStarting position (e.g 0)",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Center"] = "Center",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Center.Hint"] = "Check to center item. It works well with even and odd number of items.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Nav"] = "NAV",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Nav.Hint"] = "Check to enable next/prev buttons.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoad"] = "Lazy load",
			["Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoad.Hint"] = "Check to enable lazy load.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoadEager"] = "Lazy load eager",
			["Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoadEager.Hint"] = "Specify how many items you want to pre-load images to the right (and left when loop is enabled).",
			["Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayTimeout"] = "Auto play timeout",
			["Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayTimeout.Hint"] = "It's autoplay interval timeout. (e.g 5000)",
			["Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayHoverPause"] = "Auto play hover pause",
			["Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayHoverPause.Hint"] = "Check to enable pause on mouse hover.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.CreatedOn"] = "Created on",
			["Admin.NopStation.OCarousels.OCarousels.Fields.CreatedOn.Hint"] = "The create date of this carousel.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.UpdatedOn"] = "Updated on",
			["Admin.NopStation.OCarousels.OCarousels.Fields.UpdatedOn.Hint"] = "The last update date of this carousel.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.SelectedStoreIds"] = "Limited to stores",
			["Admin.NopStation.OCarousels.OCarousels.Fields.SelectedStoreIds.Hint"] = "Option to limit this carousel to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.ShowBackgroundPicture"] = "Show Background Picture",
			["Admin.NopStation.OCarousels.OCarousels.Fields.ShowBackgroundPicture.Hint"] = "Check to enable show Background Picture",
			["Admin.NopStation.OCarousels.OCarousels.Fields.BackgroundPicture"] = "Background Picture",
			["Admin.NopStation.OCarousels.OCarousels.Fields.BackgroundPicture.Hint"] = "Background Picture of the carousel",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Name.Required"] = "The name field is required.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Title.Required"] = "The title field is required.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.NumberOfItemsToShow.Required"] = "The number of items to show field is required.",
			["Admin.NopStation.OCarousels.OCarousels.Fields.Picture.Required"] = "The picture field is required.",
			["Admin.NopStation.OCarousels.OCarousels.List.SearchWidgetZones"] = "Widget zones",
			["Admin.NopStation.OCarousels.OCarousels.List.SearchWidgetZones.Hint"] = "The search widget zones.",
			["Admin.NopStation.OCarousels.OCarousels.List.SearchDataSources"] = "Data sources",
			["Admin.NopStation.OCarousels.OCarousels.List.SearchDataSources.Hint"] = "The search data sources.",
			["Admin.NopStation.OCarousels.OCarousels.List.SearchStore"] = "Store",
			["Admin.NopStation.OCarousels.OCarousels.List.SearchStore.Hint"] = "The search store.",
			["Admin.NopStation.OCarousels.OCarousels.List.SearchActive"] = "Active",
			["Admin.NopStation.OCarousels.OCarousels.List.SearchActive.Hint"] = "The search active.",
			["NopStation.Plugin.Widgets.OCarousels.ShopNow"] = "Shop Now",
			["NopStation.OCarousels.LoadingFailed"] = "Failed to load carousel content.",
			["NopStation.OCarousels.Items"] = "Items"
		};
	}
}
