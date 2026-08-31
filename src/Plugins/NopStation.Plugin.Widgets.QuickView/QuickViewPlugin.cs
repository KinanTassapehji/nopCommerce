using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.QuickView.Components;

namespace NopStation.Plugin.Widgets.QuickView;

public class QuickViewPlugin : BasePlugin, IWidgetPlugin, IPlugin, INopStationPlugin
{
	private readonly IWebHelper _webHelper;

	private readonly ISettingService _settingService;

	public bool HideInWidgetList => false;

	public QuickViewPlugin(IWebHelper webHelper, ISettingService settingService)
	{
		_webHelper = webHelper;
		_settingService = settingService;
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/QuickView/Configure";
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		return typeof(QuickViewViewComponent);
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		return Task.FromResult((IList<string>)new List<string> { PublicWidgetZones.Footer });
	}

	public override async Task InstallAsync()
	{
		QuickViewSettings settings = new QuickViewSettings
		{
			ShowAlsoPurchasedProducts = true,
			ShowRelatedProducts = true,
			ShowAvailability = false,
			ShowAddToWishlistButton = true,
			ShowProductEmailAFriendButton = false,
			EnablePictureZoom = true,
			ShowCompareProductsButton = false,
			ShowDeliveryInfo = false,
			ShowFullDescription = false,
			ShowProductManufacturers = false,
			ShowProductReviewOverview = false,
			ShowProductTags = false,
			ShowShortDescription = false
		};
		await _settingService.SaveSettingAsync(settings);
		await this.InstallPluginAsync();
		await base.InstallAsync();
	}

	public override async Task UninstallAsync()
	{
		await this.UninstallPluginAsync(new QuickViewPermissionConfigManager());
		await base.UninstallAsync();
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["NopStation.QuickView.Button.QuickView"] = "Quick view",
			["NopStation.QuickView.Failed"] = "Failed to load quick view",
			["Admin.NopStation.QuickView.Menu.QuickView"] = "Quick view",
			["Admin.NopStation.QuickView.Menu.Configuration"] = "Configuration",
			["Admin.NopStation.QuickView.Configuration"] = "Quick view settings",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowAlsoPurchasedProducts"] = "Show also purchased products",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowAlsoPurchasedProducts.Hint"] = "Check to show \"Also purchased products\" on quick view page.",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowRelatedProducts"] = "Show related products",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowRelatedProducts.Hint"] = "Check to show \"Related products\" on quick view page.",
			["Admin.NopStation.QuickView.Configuration.Fields.EnablePictureZoom"] = "Enable picture zoom",
			["Admin.NopStation.QuickView.Configuration.Fields.EnablePictureZoom.Hint"] = "Check to enable picture zoom. Make sure Nop-Station picture zoom plugin is installed and activated for your store.",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowShortDescription"] = "Show short description",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowShortDescription.Hint"] = "Check to show short description in quick view modal.",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowFullDescription"] = "Show full description",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowFullDescription.Hint"] = "Check to show full description in quick view modal.",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowAddToWishlistButton"] = "Show add to wishlist button",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowAddToWishlistButton.Hint"] = "Check to show 'Add To Wishlist' button in quick view modal.",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowCompareProductsButton"] = "Show compare products button",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowCompareProductsButton.Hint"] = "Check to show 'Add to compare list' button in quick view modal.",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowProductEmailAFriendButton"] = "Show product mail a friend button",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowProductEmailAFriendButton.Hint"] = "Check to show 'Email a friend' button in quick view modal.",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowProductReviewOverview"] = "Show product review overview",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowProductReviewOverview.Hint"] = "Check to show product review overview in quick view modal.",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowProductManufacturers"] = "Show product manufacturers",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowProductManufacturers.Hint"] = "Check to show product manufacturers in quick view modal.",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowAvailability"] = "Show availability",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowAvailability.Hint"] = "Check to show product availability in quick view modal.",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowDeliveryInfo"] = "Show delivery info",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowDeliveryInfo.Hint"] = "Check to show product delivery information in quick view modal.",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowProductTags"] = "Show product tags",
			["Admin.NopStation.QuickView.Configuration.Fields.ShowProductTags.Hint"] = "Check to show product tags in quick view modal.",
			["Admin.NopStation.QuickView.Configuration.Updated"] = "Quick view configuration updated successfully."
		};
	}
}
