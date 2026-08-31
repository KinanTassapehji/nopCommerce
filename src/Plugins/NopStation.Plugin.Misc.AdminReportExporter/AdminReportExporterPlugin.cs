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
using NopStation.Plugin.Misc.AdminReportExporter.Components;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.AdminReportExporter;

public class AdminReportExporterPlugin : BasePlugin, IWidgetPlugin, IPlugin, INopStationPlugin
{
	private readonly ILocalizationService _localizationService;

	private readonly ISettingService _settingService;

	private readonly IWebHelper _webHelper;

	private readonly IPermissionService _permissionService;

	public bool HideInWidgetList => false;

	public AdminReportExporterPlugin(ILocalizationService localizationService, ISettingService settingService, IWebHelper webHelper, IPermissionService permissionService)
	{
		_localizationService = localizationService;
		_settingService = settingService;
		_webHelper = webHelper;
		_permissionService = permissionService;
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/AdminReportExporter/Configure";
	}

	public override async Task InstallAsync()
	{
		await this.InstallPluginAsync();
		await base.InstallAsync();
	}

	public override async Task UninstallAsync()
	{
		await _settingService.DeleteSettingAsync<AdminReportExporterSettings>();
		await this.UninstallPluginAsync();
		await base.UninstallAsync();
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		return Task.FromResult((IList<string>)new List<string> { AdminWidgetZones.MenuBefore });
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["Admin.NopStation.AdminReportExporter.Menu.AdminReportExporter"] = "Admin report exporter",
			["Admin.NopStation.AdminReportExporter.Menu.Configuration"] = "Configuration",
			["Admin.NopStation.AdminReportExporter.Configuration.Fields.EnablePlugin"] = "Enable plugin",
			["Admin.NopStation.AdminReportExporter.Configuration.Title"] = "Configuration",
			["Admin.NopStation.AdminReportExporter.Configuration.Fields.EnablePlugin.Hint"] = "Click here to enable plugin.",
			["Admin.NopStation.AdminReportExporter.Export"] = "Export"
		};
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		return typeof(MenuReportViewComponent);
	}
}
