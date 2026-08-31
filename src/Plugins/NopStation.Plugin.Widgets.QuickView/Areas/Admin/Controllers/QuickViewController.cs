using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Plugins;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Widgets.QuickView.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.QuickView.Areas.Admin.Controllers;

public class QuickViewController : NopStationAdminController
{
	private readonly ISettingHelper<QuickViewSettings, ConfigurationModel> _settingHelper;

	private readonly IWorkContext _workContext;

	private readonly IStoreContext _storeContext;

	private readonly IPluginService _pluginService;

	public QuickViewController(ISettingHelper<QuickViewSettings, ConfigurationModel> settingHelper, IWorkContext workContext, IStoreContext storeContext, IPluginService pluginService)
	{
		_settingHelper = settingHelper;
		_workContext = workContext;
		_storeContext = storeContext;
		_pluginService = pluginService;
	}

	[CheckPermission("ManageNopStationQuickView", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		ConfigurationModel model = await _settingHelper.PrepareConfigurationModelAsync(null);
		IPluginService pluginService = _pluginService;
		model.PictureZoomPluginInstalled = await pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("NopStation.Plugin.Widgets.PictureZoom", LoadPluginsMode.InstalledOnly, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id) != null;
		return View(model);
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationQuickView", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		await _settingHelper.SaveConfigurationModelAsync(model, null, true);
		return RedirectToAction("Configure");
	}
}
