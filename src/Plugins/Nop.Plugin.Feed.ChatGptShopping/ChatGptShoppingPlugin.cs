using Nop.Core.Domain.Directory;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Feed.ChatGptShopping;

public class ChatGptShoppingPlugin : BasePlugin, IMiscPlugin
{
    #region Fields

    private readonly CurrencySettings _currencySettings;
    private readonly ILocalizationService _localizationService;
    private readonly INopUrlHelper _nopUrlHelper;
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public ChatGptShoppingPlugin(
        CurrencySettings currencySettings,
        ILocalizationService localizationService,
        INopUrlHelper nopUrlHelper,
        IScheduleTaskService scheduleTaskService,
        ISettingService settingService)
    {
        _currencySettings = currencySettings;
        _localizationService = localizationService;
        _nopUrlHelper = nopUrlHelper;
        _scheduleTaskService = scheduleTaskService;
        _settingService = settingService;
    }

    #endregion    

    #region Methods

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _nopUrlHelper.RouteUrl(ChatGptShoppingDefaults.ConfigurationRouteName);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //settings
        await _settingService.SaveSettingAsync(new ChatGptShoppingSettings
        {
            CurrencyId = _currencySettings.PrimaryStoreCurrencyId,
            StaticFileName = "chatgptshopping_products.jsonl.gz",
            ProductPictureSize = 125,
            AutoSyncEnabled = false,
            AutoSyncPeriod = ChatGptShoppingDefaults.SynchronizationTask.Period / 60,
        });

        if (await _scheduleTaskService.GetTaskByTypeAsync(ChatGptShoppingDefaults.SynchronizationTask.Type) is null)
        {
            await _scheduleTaskService.InsertTaskAsync(new()
            {
                Enabled = false,
                StopOnError = false,
                LastEnabledUtc = DateTime.UtcNow,
                Name = ChatGptShoppingDefaults.SynchronizationTask.Name,
                Type = ChatGptShoppingDefaults.SynchronizationTask.Type,
                Seconds = ChatGptShoppingDefaults.SynchronizationTask.Period
            });
        }

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Feed.ChatGptShopping.Configuration"] = "Configuration",
            ["Plugins.Feed.ChatGptShopping.Configuration.Currency"] = "Currency",
            ["Plugins.Feed.ChatGptShopping.Configuration.Currency.Hint"] = "Select the default currency that will be used to generate the feed.",
            ["Plugins.Feed.ChatGptShopping.Configuration.StaticFilePath"] = "Generated file path (static)",
            ["Plugins.Feed.ChatGptShopping.Configuration.StaticFilePath.Hint"] = "A file path of the generated file. It's static for your store and can be shared with the ChatGptShopping service.",
            ["Plugins.Feed.ChatGptShopping.Configuration.ProductPictureSize"] = "Product thumbnail image size",
            ["Plugins.Feed.ChatGptShopping.Configuration.ProductPictureSize.Hint"] = "The default size (pixels) for product thumbnail images.",
            ["Plugins.Feed.ChatGptShopping.Configuration.AutoSyncEnabled"] = "Enable auto synchronization",
            ["Plugins.Feed.ChatGptShopping.Configuration.AutoSyncEnabled.Hint"] = "Determine whether to enable auto synchronization. This will automatically synchronize changes for the products and add new ones. If disabled, synchronization must be started manually on this page.",
            ["Plugins.Feed.ChatGptShopping.Configuration.AutoSyncPeriod"] = "Auto synchronization period",
            ["Plugins.Feed.ChatGptShopping.Configuration.AutoSyncPeriod.Hint"] = "Set the period (in minutes) for auto synchronization.",
            ["Plugins.Feed.ChatGptShopping.Configuration.AutoSyncPeriod.Invalid"] = "Period is invalid",
            ["Plugins.Feed.ChatGptShopping.Generate"] = "Generate feed",
            ["Plugins.Feed.ChatGptShopping.SuccessResult"] = "ChatGPT Shopping feed has been successfully generated.",
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<ChatGptShoppingSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Feed.ChatGptShopping");

        var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(ChatGptShoppingDefaults.SynchronizationTask.Type);
        if (scheduleTask is not null)
            await _scheduleTaskService.DeleteTaskAsync(scheduleTask);

        await base.UninstallAsync();
    }

    #endregion
}
