using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Events;
using Nop.Services.Plugins;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Areas.Admin.Factories;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Domains.SMS;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services.SMS;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Controllers;

public class SmsController : NopStationAdminController
{
	private readonly ISmsModelFactory _smsModelFactory;

	private readonly ISmsPluginManager _smsPluginManager;

	private readonly IEventPublisher _eventPublisher;

	private readonly ISettingHelper<SmsSettings, SmsSettingsModel> _settingHelper;

	public SmsController(ISmsModelFactory smsModelFactory, ISmsPluginManager smsPluginManager, IEventPublisher eventPublisher, ISettingHelper<SmsSettings, SmsSettingsModel> settingHelper)
	{
		_smsModelFactory = smsModelFactory;
		_smsPluginManager = smsPluginManager;
		_eventPublisher = eventPublisher;
		_settingHelper = settingHelper;
	}

	[CheckPermission("ManageNopStationSmsConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		return View(await _settingHelper.PrepareConfigurationModelAsync(null));
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationSmsConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(SmsSettingsModel model)
	{
		await _settingHelper.SaveConfigurationModelAsync(model, null, true);
		return RedirectToAction("Configure");
	}

	[CheckPermission("ManageNopStationSmsProviders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Providers()
	{
		return View(await _smsModelFactory.PrepareSmsProviderSearchModelAsync(new SmsProviderSearchModel()));
	}

	[HttpPost]
	[CheckPermission("ManageNopStationSmsProviders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Providers(SmsProviderSearchModel searchModel)
	{
		return Json(await _smsModelFactory.PrepareSmsProviderListModelAsync(searchModel));
	}

	[HttpPost]
	[CheckPermission("ManageNopStationSmsProviders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> ProviderUpdate(SmsProviderModel model)
	{
		ISmsPlugin smsPlugin = (await _smsPluginManager.LoadSmsPluginsAsync(null, model.SystemName)).FirstOrDefault();
		if (smsPlugin == null)
		{
			return new NullJsonResult();
		}
		PluginDescriptor pluginDescriptor = smsPlugin.PluginDescriptor;
		pluginDescriptor.DisplayOrder = model.DisplayOrder;
		pluginDescriptor.Save();
		await _eventPublisher.PublishAsync(new PluginUpdatedEvent(pluginDescriptor));
		return new NullJsonResult();
	}
}
