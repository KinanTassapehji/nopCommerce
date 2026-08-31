using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Settings;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Controllers;

public class CustomerRemindersController : NopStationAdminController
{
	private readonly IConfigurationModelFactory _configurationModelFactory;

	private readonly ILocalizationService _localizationService;

	private readonly INotificationService _notificationService;

	private readonly ISettingService _settingService;

	private readonly IStoreContext _storeContext;

	public CustomerRemindersController(IConfigurationModelFactory configurationModelFactory, ILocalizationService localizationService, INotificationService notificationService, ISettingService settingService, IStoreContext storeContext)
	{
		_configurationModelFactory = configurationModelFactory;
		_localizationService = localizationService;
		_notificationService = notificationService;
		_settingService = settingService;
		_storeContext = storeContext;
	}

	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Configure()
	{
		return View(await _configurationModelFactory.PrepareConfigurationModelAsync());
	}

	[HttpPost]
	[CheckPermission("ManageCustomerReminders", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Configure(ConfigurationModel model)
	{
		if (!base.ModelState.IsValid)
		{
			return await Configure();
		}
		int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
		CustomerRemindersSettings settings = await _settingService.LoadSettingAsync<CustomerRemindersSettings>(storeScope);
		settings.IsEnabled = model.Enabled;
		settings.IsExcludeGuests = model.ExcludeGuests;
		await _settingService.SaveSettingOverridablePerStoreAsync(settings, (CustomerRemindersSettings x) => x.IsEnabled, model.Enabled_OverrideForStore, storeScope, clearCache: false);
		await _settingService.SaveSettingOverridablePerStoreAsync(settings, (CustomerRemindersSettings x) => x.IsExcludeGuests, model.ExcludeGuests_OverrideForStore, storeScope, clearCache: false);
		await _settingService.ClearCacheAsync();
		INotificationService notificationService = _notificationService;
		notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
		return await Configure();
	}
}
