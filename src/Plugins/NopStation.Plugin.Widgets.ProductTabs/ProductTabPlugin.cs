using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.ProductTabs.Components;
using NopStation.Plugin.Widgets.ProductTabs.Helpers;

namespace NopStation.Plugin.Widgets.ProductTabs;

public class ProductTabPlugin : BasePlugin, IWidgetPlugin, IPlugin, INopStationPlugin
{
	private readonly IWebHelper _webHelper;

	private readonly ILocalizationService _localizationService;

	private readonly IPermissionService _permissionService;

	private readonly ProductTabSettings _sliderSettings;

	private readonly ISettingService _settingService;

	private readonly IWorkContext _workContext;

	private readonly NopStationCoreSettings _nopStationCoreSettings;

	public bool HideInWidgetList => false;

	public ProductTabPlugin(IWebHelper webHelper, ILocalizationService localizationService, IPermissionService permissionService, ProductTabSettings sliderSettings, ISettingService settingService, IWorkContext workContext, NopStationCoreSettings nopStationCoreSettings)
	{
		_webHelper = webHelper;
		_localizationService = localizationService;
		_permissionService = permissionService;
		_sliderSettings = sliderSettings;
		_settingService = settingService;
		_workContext = workContext;
		_nopStationCoreSettings = nopStationCoreSettings;
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/ProductTab/Configure";
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		if (widgetZone == PublicWidgetZones.Footer)
		{
			return typeof(ProductTabFooterHtmlTagViewComponent);
		}
		return typeof(ProductTabViewComponent);
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		List<string> customWidgetZones = ProductTabHelper.GetCustomWidgetZones();
		customWidgetZones.Add(PublicWidgetZones.Footer);
		return Task.FromResult((IList<string>)customWidgetZones);
	}

	public override async Task InstallAsync()
	{
		await this.InstallPluginAsync();
		await base.InstallAsync();
	}

	public override async Task UninstallAsync()
	{
		await this.UninstallPluginAsync();
		await base.UninstallAsync();
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["Admin.NopStation.ProductTabs.Menu.ProductTab"] = "Product tab",
			["Admin.NopStation.ProductTabs.Menu.List"] = "List",
			["Admin.NopStation.ProductTabs.Menu.Configuration"] = "Configuration",
			["Admin.NopStation.ProductTabs.ProductTabs.List.SearchActive.Active"] = "Active",
			["Admin.NopStation.ProductTabs.ProductTabs.List.SearchActive.Inactive"] = "Inactive",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Name.Required"] = "The product tab name is required.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Title.Required"] = "The product tab title is required.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Picture.Required"] = "The product tab picture is required.",
			["Admin.NopStation.ProductTabs.ProductTabItems.Fields.Name.Required"] = "The product tab item name is required.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Name"] = "Name",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Name.Hint"] = "The name of the product tab.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Title"] = "Title",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Title.Hint"] = "The title of the product tab.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.DisplayTitle"] = "Display title",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.DisplayTitle.Hint"] = "Check to display title.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Picture"] = "Picture",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Picture.Hint"] = "Select product tab picture.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.DisplayOrder"] = "Display Order",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.DisplayOrder.Hint"] = "Display order of the product tab. 1 represents the top of the list.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Active"] = "Active",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Active.Hint"] = "Determines whether product tab is active or not.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.CustomUrl"] = "Custom URL",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.CustomUrl.Hint"] = "The custom url.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.WidgetZone"] = "Widget zone",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.WidgetZone.Hint"] = "The widget-zone of the product tab.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.AutoPlay"] = "Auto play",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.AutoPlay.Hint"] = "Check to enable auto-play.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.CustomCssClass"] = "Custom CSS Class",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.CustomCssClass.Hint"] = "Enter the custom CSS class to be applied.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Loop"] = "Loop",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Loop.Hint"] = "Check to enable 'infinity loop' which duplicates last and first items to get loop illusion. (e.g false)",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Margin"] = "Margin",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Margin.Hint"] = "It's margin-right(px) on item. (Default 0)",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.StartPosition"] = "Starting position",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.StartPosition.Hint"] = "Starting position (e.g 0)",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Center"] = "Center",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Center.Hint"] = "Check to center item. It works well with even an odd number of items. (e.g false)",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Nav"] = "NAV",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.Nav.Hint"] = "Check to enable next/prev buttons. (e.g false)",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.LazyLoad"] = "Lazy load",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.LazyLoad.Hint"] = "Check to enable lazy-load images (e.g false)",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.LazyLoadEager"] = "Lazy load eager",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.LazyLoadEager.Hint"] = "Check to eagerly pre-load images to the right (and left when loop is enabled) based on how many items you want to preload.",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.AutoPlayTimeout"] = "Auto play timeout",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.AutoPlayTimeout.Hint"] = "It's autoplay interval timeout. (e.g 5000)",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.AutoPlayHoverPause"] = "Auto play hover pause",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.AutoPlayHoverPause.Hint"] = "Check to enable pause on mouse hover. (e.g false)",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.SelectedStoreIds"] = "Limited to stores",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.SelectedStoreIds.Hint"] = "Option to limit this product tab to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty.",
			["Admin.NopStation.ProductTabs.ProductTabs.Deleted"] = "Product tab deleted successfully.",
			["Admin.NopStation.ProductTabs.ProductTabs.Updated"] = "Product tab updated successfully.",
			["Admin.NopStation.ProductTabs.ProductTabs.Created"] = "Product tab created successfully.",
			["Admin.NopStation.ProductTabs.ProductTabItems.Fields.Name"] = "Name",
			["Admin.NopStation.ProductTabs.ProductTabItems.Fields.Name.Hint"] = "Product tab item name.",
			["Admin.NopStation.ProductTabs.ProductTabItems.Fields.DisplayOrder"] = "Display order",
			["Admin.NopStation.ProductTabs.ProductTabItems.Fields.DisplayOrder.Hint"] = "Display order of the product tab item. 1 represents the top of the list.",
			["Admin.NopStation.ProductTabs.ProductTabItems.Updated"] = "Product tab item updated successfully",
			["Admin.NopStation.ProductTabs.ProductTabItems.Created"] = "Product tab item created successfully",
			["Admin.NopStation.ProductTabs.ProductTabs.Tab.Info"] = "Info",
			["Admin.NopStation.ProductTabs.ProductTabs.Tab.Properties"] = "Properties",
			["Admin.NopStation.ProductTabs.ProductTabs.Tab.ProductTabItems"] = "Product tab items",
			["Admin.NopStation.ProductTabs.ProductTabs.ProductTabItems.BtnAddNew"] = "Add new tab item",
			["Admin.NopStation.ProductTabs.ProductTabs.ProductTabItems.SaveBeforeEdit"] = "You need to save the product tab before you can add item for this product tab page.",
			["Admin.NopStation.ProductTabs.ProductTabItems.Tab.Info"] = "Info",
			["Admin.NopStation.ProductTabs.ProductTabItems.Tab.Products"] = "Products",
			["Admin.NopStation.ProductTabs.ProductTabItems.ProductTabItemProducts.BtnAddNew"] = "Add new product",
			["Admin.NopStation.ProductTabs.ProductTabItems.ProductTabItemProducts.SaveBeforeEdit"] = "You need to save the product tab item before you can add product for this product tab item page.",
			["Admin.NopStation.ProductTabs.Configuration"] = "Product tab settings",
			["Admin.NopStation.ProductTabs.ProductTabs.AddNew"] = "Add new product tab",
			["Admin.NopStation.ProductTabs.ProductTabs.EditDetails"] = "Edit product tab",
			["Admin.NopStation.ProductTabs.ProductTabs.BackToList"] = "back to tab list",
			["Admin.NopStation.ProductTabs.ProductTabItems.AddNew"] = "Add new tab item",
			["Admin.NopStation.ProductTabs.ProductTabItems.EditDetails"] = "Edit tab item",
			["Admin.NopStation.ProductTabs.ProductTabItems.BackToProductTab"] = "back to product tab",
			["Admin.NopStation.ProductTabs.ProductTabList"] = "Product tabs",
			["Admin.NopStation.ProductTabs.ProductTabItems.Products.AddNew"] = "Add new products",
			["Admin.NopStation.ProductTabs.Configuration.Fields.EnableProductTab"] = "Enable product tab",
			["Admin.NopStation.ProductTabs.Configuration.Fields.EnableProductTab.Hint"] = "Check to enable product tab.",
			["Admin.NopStation.ProductTabs.ProductTabItemProducts.Fields.ProductTabItem"] = "Tab item",
			["Admin.NopStation.ProductTabs.ProductTabItemProducts.Fields.Product"] = "Product",
			["Admin.NopStation.ProductTabs.ProductTabItemProducts.Fields.DisplayOrder"] = "Display order",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.CreatedOn"] = "CreatedOn Utc",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.CreatedOn.Hint"] = "Created Date",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.UpdatedOn"] = "UpdatedOn Utc",
			["Admin.NopStation.ProductTabs.ProductTabs.Fields.UpdatedOn.Hint"] = "Updated Date",
			["Admin.NopStation.ProductTabs.Configuration.Updated"] = "Configuration Has been Updated"
		};
	}
}
