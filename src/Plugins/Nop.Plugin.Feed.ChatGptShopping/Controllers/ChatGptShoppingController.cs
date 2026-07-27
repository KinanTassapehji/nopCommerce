using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Plugin.Feed.ChatGptShopping.Models;
using Nop.Plugin.Feed.ChatGptShopping.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Feed.ChatGptShopping.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class ChatGptShoppingController : BasePluginController
{
    #region Fields

    private readonly ChatGptShoppingService _chatGptShoppingService;
    private readonly ICurrencyService _currencyService;
    private readonly ILocalizationService _localizationService;
    private readonly INopFileProvider _nopFileProvider;
    private readonly INotificationService _notificationService;
    private readonly ILogger _logger;
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _storeService;
    private readonly IWebHelper _webHelper;
    private readonly IWebHostEnvironment _webHostEnvironment;

    #endregion

    #region Ctor

    public ChatGptShoppingController(ChatGptShoppingService chatGptShoppingService,
        ICurrencyService currencyService,
        ILocalizationService localizationService,
        INopFileProvider nopFileProvider,
        INotificationService notificationService,
        ILogger logger,
        IScheduleTaskService scheduleTaskService,
        ISettingService settingService,
        IStoreContext storeContext,
        IStoreService storeService,
        IWebHelper webHelper,
        IWebHostEnvironment webHostEnvironment)
    {
        _chatGptShoppingService = chatGptShoppingService;
        _currencyService = currencyService;
        _localizationService = localizationService;
        _nopFileProvider = nopFileProvider;
        _notificationService = notificationService;
        _logger = logger;
        _scheduleTaskService = scheduleTaskService;
        _settingService = settingService;
        _storeContext = storeContext;
        _storeService = storeService;
        _webHelper = webHelper;
        _webHostEnvironment = webHostEnvironment;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure()
    {
        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var chatGptShoppingSettings = await _settingService.LoadSettingAsync<ChatGptShoppingSettings>(storeScope);

        //prepare model
        var model = new ConfigurationModel
        {
            CurrencyId = chatGptShoppingSettings.CurrencyId,
            ProductPictureSize = chatGptShoppingSettings.ProductPictureSize,
            AutoSyncEnabled = chatGptShoppingSettings.AutoSyncEnabled,
            AutoSyncPeriod = chatGptShoppingSettings.AutoSyncPeriod,
        };

        foreach (var currency in await _currencyService.GetAllCurrenciesAsync())
            model.AvailableCurrencies.Add(new SelectListItem { Text = currency.Name, Value = currency.Id.ToString() });

        //file paths
        foreach (var store in await _storeService.GetAllStoresAsync())
        {
            var localFilePath = _nopFileProvider.Combine(_webHostEnvironment.WebRootPath, "files", "exportimport", store.Id + "-" + chatGptShoppingSettings.StaticFileName);
            if (_nopFileProvider.FileExists(localFilePath))
            {
                model.GeneratedFiles.Add(new GeneratedFileModel
                {
                    StoreName = store.Name,
                    FileUrl = $"{_webHelper.GetStoreLocation(false)}files/exportimport/{store.Id}-{chatGptShoppingSettings.StaticFileName}"
                });
            }
        }

        model.ActiveStoreScopeConfiguration = storeScope;
        if (storeScope > 0)
        {
            model.CurrencyId_OverrideForStore = await _settingService.SettingExistsAsync(chatGptShoppingSettings, x => x.CurrencyId, storeScope);
            model.ProductPictureSize_OverrideForStore = await _settingService.SettingExistsAsync(chatGptShoppingSettings, x => x.ProductPictureSize, storeScope);
            model.AutoSyncEnabled_OverrideForStore = await _settingService.SettingExistsAsync(chatGptShoppingSettings, x => x.AutoSyncEnabled, storeScope);
            model.AutoSyncPeriod_OverrideForStore = await _settingService.SettingExistsAsync(chatGptShoppingSettings, x => x.AutoSyncPeriod, storeScope);
        }

        return View("~/Plugins/Feed.ChatGptShopping/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [FormValueRequired("save")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var chatGptShoppingSettings = await _settingService.LoadSettingAsync<ChatGptShoppingSettings>(storeScope);

        //set new settings values
        chatGptShoppingSettings.CurrencyId = model.CurrencyId;
        chatGptShoppingSettings.AutoSyncEnabled = model.AutoSyncEnabled;
        chatGptShoppingSettings.AutoSyncPeriod = model.AutoSyncPeriod;
        chatGptShoppingSettings.ProductPictureSize = model.ProductPictureSize;
        /* We do not clear cache after each setting update.
         * This behavior can increase performance because cached settings will not be cleared 
         * and loaded from database after each update */
        await _settingService.SaveSettingOverridablePerStoreAsync(chatGptShoppingSettings, x => x.CurrencyId, model.CurrencyId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(chatGptShoppingSettings, x => x.AutoSyncEnabled, model.AutoSyncEnabled_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(chatGptShoppingSettings, x => x.AutoSyncPeriod, model.AutoSyncPeriod_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(chatGptShoppingSettings, x => x.ProductPictureSize, model.ProductPictureSize_OverrideForStore, storeScope, false);

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(ChatGptShoppingDefaults.SynchronizationTask.Type);
        if (scheduleTask is not null)
        {
            if (!scheduleTask.Enabled && chatGptShoppingSettings.AutoSyncEnabled)
                scheduleTask.LastEnabledUtc = DateTime.UtcNow;
            scheduleTask.Enabled = chatGptShoppingSettings.AutoSyncEnabled;
            scheduleTask.Seconds = chatGptShoppingSettings.AutoSyncPeriod * 60;
            await _scheduleTaskService.UpdateTaskAsync(scheduleTask);
        }

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return RedirectToAction(nameof(Configure));
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("generate")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> GenerateFeed(ChatGptShoppingSettings model)
    {
        try
        {
            await _chatGptShoppingService.GenerateChatGptFeedAsync(false);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Feed.ChatGptShopping.SuccessResult"));
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
            await _logger.ErrorAsync(exc.Message, exc);
        }

        return await Configure();
    }

    #endregion
}
