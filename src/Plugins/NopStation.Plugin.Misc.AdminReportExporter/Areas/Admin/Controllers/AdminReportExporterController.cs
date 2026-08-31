using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.AdminReportExporter.Areas.Admin.Model;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Misc.AdminReportExporter.Areas.Admin.Controllers;

public class AdminReportExporterController : NopStationAdminController
{
	private readonly ILocalizationService _localizationService;

	private readonly INotificationService _notificationService;

	private readonly IStoreContext _storeContext;

	private readonly ISettingService _settingService;

	private readonly ISettingHelper<AdminReportExporterSettings, ConfigurationModel> _settingsHelper;

	public AdminReportExporterController(ILocalizationService localizationService, INotificationService notificationService, IStoreContext storeContext, ISettingService settingService, ISettingHelper<AdminReportExporterSettings, ConfigurationModel> settingsHelper)
	{
		_localizationService = localizationService;
		_notificationService = notificationService;
		_storeContext = storeContext;
		_settingService = settingService;
		_settingsHelper = settingsHelper;
	}

	[CheckPermission("ManageAdminReportExporterConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		return View(await _settingsHelper.PrepareConfigurationModelAsync(null));
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageAdminReportExporterConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		await _settingsHelper.SaveConfigurationModelAsync(model, null, true);
		return RedirectToAction("Configure");
	}
}
