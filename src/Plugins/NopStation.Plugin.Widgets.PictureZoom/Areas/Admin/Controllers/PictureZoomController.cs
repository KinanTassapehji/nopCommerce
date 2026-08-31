using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Widgets.PictureZoom.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PictureZoom.Infrastructure.Cache;

namespace NopStation.Plugin.Widgets.PictureZoom.Areas.Admin.Controllers;

public class PictureZoomController : NopStationAdminController
{
	private readonly ISettingHelper<PictureZoomSettings, ConfigurationModel> _settingHelper;

	private readonly IStaticCacheManager _cacheManager;

	public PictureZoomController(ISettingHelper<PictureZoomSettings, ConfigurationModel> settingHelper, IStaticCacheManager staticCacheManager)
	{
		_settingHelper = settingHelper;
		_cacheManager = staticCacheManager;
	}

	[CheckPermission("ManageNopStationPictureZoom", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		return View(await _settingHelper.PrepareConfigurationModelAsync(null));
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationPictureZoom", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		await _settingHelper.SaveConfigurationModelAsync(model, null, true);
		await _cacheManager.RemoveByPrefixAsync(ModelCacheEventConsumer.PrictureZoom_patern_key);
		return RedirectToAction("Configure");
	}
}
