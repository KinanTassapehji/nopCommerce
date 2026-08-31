using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Configuration;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Settings;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;

public class ConfigurationModelFactory : IConfigurationModelFactory
{
	private readonly ISettingService _settingService;

	private readonly IStoreContext _storeContext;

	public ConfigurationModelFactory(ISettingService settingService, IStoreContext storeContext)
	{
		_settingService = settingService;
		_storeContext = storeContext;
	}

	public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
	{
		int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
		CustomerRemindersSettings settings = await _settingService.LoadSettingAsync<CustomerRemindersSettings>(storeScope);
		ConfigurationModel model = new ConfigurationModel
		{
			Enabled = settings.IsEnabled,
			ExcludeGuests = settings.IsExcludeGuests,
			ActiveStoreScopeConfiguration = storeScope
		};
		if (storeScope > 0)
		{
			ConfigurationModel configurationModel = model;
			configurationModel.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(settings, (CustomerRemindersSettings x) => x.IsEnabled, storeScope);
			configurationModel = model;
			configurationModel.ExcludeGuests_OverrideForStore = await _settingService.SettingExistsAsync(settings, (CustomerRemindersSettings x) => x.IsExcludeGuests, storeScope);
		}
		return model;
	}
}
