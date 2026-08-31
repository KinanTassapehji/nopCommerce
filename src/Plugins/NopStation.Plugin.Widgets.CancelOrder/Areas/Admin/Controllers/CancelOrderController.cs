using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Widgets.CancelOrder.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.CancelOrder.Areas.Admin.Controllers;

public class CancelOrderController : NopStationAdminController
{
	private readonly ISettingHelper<CancelOrderSettings, ConfigurationModel> _settingHelper;

	private readonly IBaseAdminModelFactory _baseAdminModelFactory;

	public CancelOrderController(ISettingHelper<CancelOrderSettings, ConfigurationModel> settingHelper, IBaseAdminModelFactory baseAdminModelFactory)
	{
		_settingHelper = settingHelper;
		_baseAdminModelFactory = baseAdminModelFactory;
	}

	[CheckPermission("ManageNopStationCancelOrder", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		ConfigurationModel model = await _settingHelper.PrepareConfigurationModelAsync(null);
		await _baseAdminModelFactory.PrepareOrderStatusesAsync(model.AvailableOrderStatuses, withSpecialDefaultItem: false);
		await _baseAdminModelFactory.PreparePaymentStatusesAsync(model.AvailablePaymentStatuses, withSpecialDefaultItem: false);
		await _baseAdminModelFactory.PrepareShippingStatusesAsync(model.AvailableShippingStatuses, withSpecialDefaultItem: false);
		return View(model);
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationCancelOrder", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		await _settingHelper.SaveConfigurationModelAsync(model, null, true);
		return RedirectToAction("Configure");
	}
}
