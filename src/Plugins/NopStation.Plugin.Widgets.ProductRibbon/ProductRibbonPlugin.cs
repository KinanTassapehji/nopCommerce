using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.ProductRibbon.Components;

namespace NopStation.Plugin.Widgets.ProductRibbon;

public class ProductRibbonPlugin : BasePlugin, IWidgetPlugin, IPlugin, INopStationPlugin
{
	private readonly IWebHelper _webHelper;

	private readonly ProductRibbonSettings _productRibbonSettings;

	private readonly ISettingService _settingService;

	public bool HideInWidgetList => false;

	public ProductRibbonPlugin(IWebHelper webHelper, ProductRibbonSettings productRibbonSettings, ISettingService settingService)
	{
		_webHelper = webHelper;
		_productRibbonSettings = productRibbonSettings;
		_settingService = settingService;
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/ProductRibbon/Configure";
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		if (widgetZone == PublicWidgetZones.Footer)
		{
			return typeof(ProductRibbonFooterViewComponent);
		}
		return typeof(ProductRibbonViewComponent);
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		string item = (string.IsNullOrWhiteSpace(_productRibbonSettings.ProductDetailsPageWidgetZone) ? PublicWidgetZones.ProductDetailsBeforePictures : _productRibbonSettings.ProductDetailsPageWidgetZone);
		string item2 = (string.IsNullOrWhiteSpace(_productRibbonSettings.ProductOverviewBoxWidgetZone) ? PublicWidgetZones.ProductBoxAddinfoBefore : _productRibbonSettings.ProductOverviewBoxWidgetZone);
		return Task.FromResult((IList<string>)new List<string>
		{
			item,
			item2,
			PublicWidgetZones.Footer
		});
	}

	public override async Task InstallAsync()
	{
		ProductRibbonSettings settings = new ProductRibbonSettings
		{
			EnableBestSellerRibbon = true,
			EnableDiscountRibbon = true,
			EnableNewRibbon = true,
			ProductDetailsPageWidgetZone = PublicWidgetZones.ProductDetailsBeforePictures,
			ProductOverviewBoxWidgetZone = PublicWidgetZones.ProductBoxAddinfoBefore,
			BestSellStoreWise = true,
			SoldInDays = 30,
			BestSellOrderStatusIds = new List<int> { 30, 30 },
			BestSellPaymentStatusIds = new List<int> { 30 },
			BestSellShippingStatusIds = new List<int> { 40, 30, 10 }
		};
		await _settingService.SaveSettingAsync(settings);
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
			["Admin.NopStation.ProductRibbon.Menu.ProductRibbon"] = "Product ribbon",
			["Admin.NopStation.ProductRibbon.Menu.Configuration"] = "Configuration",
			["Admin.NopStation.ProductRibbon.Configuration"] = "Product ribbon settings",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.EnableNewRibbon"] = "Enable 'New' ribbon",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.EnableDiscountRibbon"] = "Enable 'Discount' ribbon",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.EnableBestSellerRibbon"] = "Enable 'Best Seller' ribbon",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.EnableNewRibbon.Hint"] = "Check to enable 'New' ribbon on product view (product overview box and details page).",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.EnableDiscountRibbon.Hint"] = "Check to enable 'Discount' ribbon on product view (product overview box and details page)",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.EnableBestSellerRibbon.Hint"] = "Check to enable 'Best Seller' ribbon on product view (product overview box and details page)",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.ProductDetailsPageWidgetZone.Required"] = "Product details page widget zone is required.",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.ProductOverviewBoxWidgetZone.Required"] = "Product overview box widget zone is required.",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.ProductDetailsPageWidgetZone"] = "Product details page widget zone",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.ProductDetailsPageWidgetZone.Hint"] = "Specify the widget zone where the ribbon will be appeared in product details page. (i.e. productdetails_before_pictures)",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.ProductOverviewBoxWidgetZone"] = "Product overview box widget zone",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.ProductOverviewBoxWidgetZone.Hint"] = "Specify the widget zone where the ribbon will be appeared in product overview box. (i.e. productbox_addinfo_before)",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.SoldInDays"] = "Sold in days",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.SoldInDays.Hint"] = "Sold in days (i.e. 10, 30).",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellStoreWise"] = "Best sell store-wise",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellStoreWise.Hint"] = "Check to calculate best selling product per store.",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellPaymentStatus"] = "Best sell payment status",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellPaymentStatus.Hint"] = "Select best sell payment status options.",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellOrderStatus"] = "Best sell order status",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellOrderStatus.Hint"] = "Select best sell order status options.",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellShippingStatus"] = "Best sell shipping status",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellShippingStatus.Hint"] = "Select best sell shipping status options.",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.MinimumAmountSold"] = "Minimum amount sold",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.MinimumAmountSold.Hint"] = "Enter minimum amount of sell to be marked as best seller.",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.MinimumQuantitySold"] = "Minimum quantity sold",
			["Admin.NopStation.ProductRibbon.Configuration.Fields.MinimumQuantitySold.Hint"] = "Enter minimum quantity of sell to be marked as best seller.",
			["Admin.NopStation.ProductRibbon.Configuration.Updated"] = "Product ribbon settings updated successfully.",
			["NopStation.ProductRibbon.RibbonText.New"] = "New",
			["NopStation.ProductRibbon.RibbonText.Discount"] = "{0}% Off",
			["NopStation.ProductRibbon.RibbonText.BestSeller"] = "Best Seller"
		};
	}
}
