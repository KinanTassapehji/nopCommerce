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
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.PrevNextProduct.Components;

namespace NopStation.Plugin.Widgets.PrevNextProduct;

public class PrevNextProductPlugin : BasePlugin, IWidgetPlugin, IPlugin, INopStationPlugin
{
	private readonly ISettingService _settingService;

	private readonly PrevNextProductSettings _prevNextProductSettings;

	private readonly IPermissionService _permissionService;

	private readonly ILocalizationService _localizationService;

	private readonly IWebHelper _webHelper;

	public bool HideInWidgetList => false;

	public PrevNextProductPlugin(ISettingService settingService, PrevNextProductSettings prevNextProductSettings, IPermissionService permissionService, ILocalizationService localizationService, IWebHelper webHelper)
	{
		_prevNextProductSettings = prevNextProductSettings;
		_permissionService = permissionService;
		_localizationService = localizationService;
		_webHelper = webHelper;
		_settingService = settingService;
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/PrevNextProduct/Configure";
	}

	public override async Task InstallAsync()
	{
		await _settingService.SaveSettingAsync(new PrevNextProductSettings
		{
			WidgetZone = PublicWidgetZones.ProductDetailsTop,
			EnableLoop = true,
			NavigateBasedOnId = 0,
			ProductNameMaxLength = 30,
			ProductThumbnailSize = 100
		});
		await this.InstallPluginAsync();
		await base.InstallAsync();
	}

	public override async Task UninstallAsync()
	{
		await this.UninstallPluginAsync(new PrevNextProductPermissionConfigManager());
		await base.UninstallAsync();
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["Admin.NopStation.PrevNextProduct.Menu.PrevNextProduct"] = "Prev/Next product",
			["Admin.NopStation.PrevNextProduct.Menu.Configuration"] = "Configuration",
			["Admin.NopStation.PrevNextProduct.Configuration"] = "Prev/Next product settings",
			["Admin.NopStation.PrevNextProduct.Configuration.Fields.EnableLoop"] = "Enable loop",
			["Admin.NopStation.PrevNextProduct.Configuration.Fields.EnableLoop.Hint"] = "Check to enable loop. This will allow to show first product of specified catalog (i.e. Category, Manufacturer) page as 'Next product' when browing last product of that catalog. Also it will show last product as 'Previous product' when browsing the first product.",
			["Admin.NopStation.PrevNextProduct.Configuration.Fields.WidgetZone"] = "Widget zone",
			["Admin.NopStation.PrevNextProduct.Configuration.Fields.WidgetZone.Hint"] = "The widget zone of previous/next buttons in product details page.",
			["Admin.NopStation.PrevNextProduct.Configuration.Fields.NavigateBasedOn"] = "Navigate based on",
			["Admin.NopStation.PrevNextProduct.Configuration.Fields.NavigateBasedOn.Hint"] = "Navigate previous/next product based on catalog type.",
			["Admin.NopStation.PrevNextProduct.Configuration.Fields.ProductNameMaxLength"] = "Product name max length",
			["Admin.NopStation.PrevNextProduct.Configuration.Fields.ProductNameMaxLength.Hint"] = "The maximum length of product name to show in previous/next buttons.",
			["Admin.NopStation.PrevNextProduct.Configuration.Fields.ProductThumbnailSize"] = "Product Thumb Size",
			["Admin.NopStation.PrevNextProduct.Configuration.Fields.ProductThumbnailSize.Hint"] = "The thumbnail size of product image to show in previous/next buttons.",
			["NopStation.PrevNextProduct.PreviousProduct"] = "Previous Product",
			["NopStation.PrevNextProduct.NextProduct"] = "Next Product",
			["NopStation.PrevNextProduct.PreviousProduct.Name"] = "{0}",
			["NopStation.PrevNextProduct.NextProduct.Name"] = "{0}"
		};
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		string item = (string.IsNullOrWhiteSpace(_prevNextProductSettings.WidgetZone) ? PublicWidgetZones.ProductDetailsTop : _prevNextProductSettings.WidgetZone);
		return Task.FromResult((IList<string>)new List<string> { item });
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		return typeof(PrevNextProductViewComponent);
	}
}
