using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.Product360View.Components;

namespace NopStation.Plugin.Widgets.Product360View;

public class Product360ViewPlugin : BasePlugin, IWidgetPlugin, IPlugin, INopStationPlugin
{
	private readonly ILocalizationService _localizationService;

	private readonly IWebHelper _webHelper;

	private readonly IPermissionService _permissionService;

	public bool HideInWidgetList => false;

	public Product360ViewPlugin(ILocalizationService localizationService, IWebHelper webHelper, IPermissionService permissionService)
	{
		_localizationService = localizationService;
		_webHelper = webHelper;
		_permissionService = permissionService;
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/Product360View/Configure";
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		return typeof(Product360ViewComponent);
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		return Task.FromResult((IList<string>)new List<string>
		{
			AdminWidgetZones.ProductDetailsBlock,
			PublicWidgetZones.ProductDetailsAfterPictures
		});
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
			["Plugins.Widgets.Product360View.Menu.Product360View"] = "Product 360 View",
			["Plugins.Widgets.Product360View.Menu.Configuration"] = "Configuration",
			["Plugins.Widgets.Product360View.IsEnabled"] = "Is enabled?",
			["Plugins.Widgets.Product360View.IsEnabled.Hint"] = "Determine is enabled or not.",
			["Plugins.Widgets.Product360View.Fields.ProductId"] = "Product",
			["Plugins.Widgets.Product360View.Fields.DisplayOrder"] = "Display order",
			["Plugins.Widgets.Product360View.Fields.PictureId"] = "Picture",
			["Plugins.Widgets.Product360View.Fields.OverrideAltAttribute"] = "Alt",
			["Plugins.Widgets.Product360View.Fields.OverrideTitleAttribute"] = "Title",
			["Plugins.Widgets.Product360View.Fields.BehaviorTypeId"] = "Behavior type",
			["Plugins.Widgets.Product360View.Fields.BehaviorTypeId.Hint"] = "Specify the Behavior type. Select Mouse Drag, Mouse Movements or Mouse Wheel to move 360 image (animation will disabled in mouse wheel)",
			["Plugins.Widgets.Product360View.Fields.IsLoopEnabled"] = "Is loop enabled?",
			["Plugins.Widgets.Product360View.Fields.IsLoopEnabled.Hint"] = "Determine is loop enabled or not. It will be continuously spinning if it is enabled.",
			["Plugins.Widgets.Product360View.Fields.IsZoomEnabled.Hint"] = "Determine is zoom enabled or not. Double click on 360 image will Show/Hide zoom view",
			["Plugins.Widgets.Product360View.Fields.IsZoomEnabled"] = "Is zoom enabled?",
			["Plugins.Widgets.Product360View.Fields.IsPanoramaEnabled"] = "Is panorama enabled?",
			["Plugins.Widgets.Product360View.Fields.IsPanoramaEnabled.Hint"] = "Click to enable Panorama View and upload a panorama image. Image with minimum display order will be selected if there are multiple images.",
			["Plugins.Widgets.Product360View.360Picture"] = "360 Pictures",
			["Plugins.Widgets.Product360View.PanoramaPicture"] = "Panorama Pictures"
		};
	}
}
