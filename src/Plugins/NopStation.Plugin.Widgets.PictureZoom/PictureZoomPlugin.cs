using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.PictureZoom.Components;

namespace NopStation.Plugin.Widgets.PictureZoom;

public class PictureZoomPlugin : BasePlugin, INopStationPlugin, IPlugin, IWidgetPlugin
{
	private readonly IWebHelper _webHelper;

	private readonly ISettingService _settingService;

	private readonly ILocalizationService _localizationService;

	public bool HideInWidgetList => false;

	public PictureZoomPlugin(IWebHelper webHelper, ISettingService settingService, ILocalizationService localizationService)
	{
		_webHelper = webHelper;
		_settingService = settingService;
		_localizationService = localizationService;
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/PictureZoom/Configure";
	}

	public override async Task InstallAsync()
	{
		PictureZoomSettings settings = new PictureZoomSettings
		{
			EnablePictureZoom = true
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

	public override async Task UpdateAsync(string currentVersion, string targetVersion)
	{
		await _localizationService.AddOrUpdateLocaleResourceAsync(GetPluginResources());
		await base.UpdateAsync(currentVersion, targetVersion);
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["Admin.NopStation.PictureZoom.Menu.PictureZoom"] = "Picture zoom",
			["Admin.NopStation.PictureZoom.Menu.Configuration"] = "Configuration",
			["Admin.NopStation.PictureZoom.Configuration.Fields.EnablePictureZoom"] = "Enable picture zoom",
			["Admin.NopStation.PictureZoom.Configuration.Fields.EnablePictureZoom.Hint"] = "Check to enable picture zoom.",
			["Admin.NopStation.PictureZoom.Configuration.Fields.ZoomWidth"] = "Zoom width",
			["Admin.NopStation.PictureZoom.Configuration.Fields.ZoomWidth.Hint"] = "Picture zoom width ratio according to picture.",
			["Admin.NopStation.PictureZoom.Configuration.Fields.ZoomHeight"] = "Zoom height",
			["Admin.NopStation.PictureZoom.Configuration.Fields.ZoomHeight.Hint"] = "Picture width width ratio according to picture.",
			["Admin.NopStation.PictureZoom.Configuration.Fields.LtrPositionTypeId"] = "Ltr position type",
			["Admin.NopStation.PictureZoom.Configuration.Fields.LtrPositionTypeId.Hint"] = "Picture zoom postion for left-to-right language (eg. top, right, inside. default: right)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.RtlPositionTypeId"] = "Rtl position type",
			["Admin.NopStation.PictureZoom.Configuration.Fields.RtlPositionTypeId.Hint"] = "Picture zoom postion for right-to-left language (eg. top, right, inside. default: left)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.Tint"] = "Tint",
			["Admin.NopStation.PictureZoom.Configuration.Fields.Tint.Hint"] = "Tint. (e.g false)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.TintOpacity"] = "Tint opacity",
			["Admin.NopStation.PictureZoom.Configuration.Fields.TintOpacity.Hint"] = "Tint Opacity (e.g 0.5)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.LensOpacity"] = "Lens opacity",
			["Admin.NopStation.PictureZoom.Configuration.Fields.LensOpacity.Hint"] = "Lens Opacity (e.g 0.5)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.SoftFocus"] = "Soft focus",
			["Admin.NopStation.PictureZoom.Configuration.Fields.SoftFocus.Hint"] = "Soft Focus (e.g false)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.SmoothMove"] = "Smooth move",
			["Admin.NopStation.PictureZoom.Configuration.Fields.SmoothMove.Hint"] = "Smooth Move (e.g 3)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.ShowTitle"] = "Show title",
			["Admin.NopStation.PictureZoom.Configuration.Fields.ShowTitle.Hint"] = "Show Title (e.g true)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.TitleOpacity"] = "Title opacity",
			["Admin.NopStation.PictureZoom.Configuration.Fields.TitleOpacity.Hint"] = "Title Opacity (e.g 0.5)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.AdjustX"] = "Adjust X",
			["Admin.NopStation.PictureZoom.Configuration.Fields.AdjustX.Hint"] = "AdjustX (e.g 0)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.AdjustY"] = "Adjust Y",
			["Admin.NopStation.PictureZoom.Configuration.Fields.AdjustY.Hint"] = "AdjustY (e.g 0)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.ImageSize"] = "Image size",
			["Admin.NopStation.PictureZoom.Configuration.Fields.ImageSize.Hint"] = "Image Size (e.g 500)",
			["Admin.NopStation.PictureZoom.Configuration.Fields.FullSizeImage"] = "Full size image",
			["Admin.NopStation.PictureZoom.Configuration.Fields.FullSizeImage.Hint"] = "Full Size Image (e.g 1000)",
			["Admin.NopStation.PictureZoom.Configuration"] = "Picture zoom settings"
		};
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		return Task.FromResult((IList<string>)new List<string> { PublicWidgetZones.Footer });
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		return typeof(PictureZoomViewComponent);
	}
}
