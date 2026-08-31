using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Widgets.PrevNextProduct.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PrevNextProduct.Domains;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Areas.Admin.Controller;

public class PrevNextProductController : NopStationAdminController
{
	private readonly ISettingHelper<PrevNextProductSettings, ConfigurationModel> _settingHelper;

	public PrevNextProductController(ISettingHelper<PrevNextProductSettings, ConfigurationModel> settingHelper)
	{
		_settingHelper = settingHelper;
	}

	[CheckPermission("ManageNopStationPrevNextProduct", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		ConfigurationModel model = await _settingHelper.PrepareConfigurationModelAsync(null);
		ConfigurationModel configurationModel = model;
		configurationModel.AvailableNavigationTypes = (await NavigationType.Category.ToSelectListAsync()).ToList();
		return View(model);
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationPrevNextProduct", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		await _settingHelper.SaveConfigurationModelAsync(model, null, true);
		return RedirectToAction("Configure");
	}
}
