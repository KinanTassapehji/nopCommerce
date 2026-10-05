using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Syriatel.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Syriatel.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class PaymentSyriatelController : BasePaymentController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;


    #endregion

    #region Ctor

    public PaymentSyriatelController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;

    }

    #endregion

    #region Methods


    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure()
    {
        var syriatelPaymentSettings = await _settingService.LoadSettingAsync<SyriatelPaymentSettings>();

        var model = new ConfigurationModel
        {
            BaseUrl = syriatelPaymentSettings.BaseUrl,
            Username = syriatelPaymentSettings.Username,
            Password = syriatelPaymentSettings.Password,
            MerchantMsisdn = syriatelPaymentSettings.MerchantMsisdn,
            AdditionalFee = syriatelPaymentSettings.AdditionalFee,
            AdditionalFeePercentage = syriatelPaymentSettings.AdditionalFeePercentage,
        };

        return View("~/Plugins/Payments.Syriatel/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();
        var syriatelPaymentSettings = await _settingService.LoadSettingAsync<SyriatelPaymentSettings>();

        //save settings
        syriatelPaymentSettings.BaseUrl = model.BaseUrl;
        syriatelPaymentSettings.Username = model.Username;
        syriatelPaymentSettings.Password = model.Password;
        syriatelPaymentSettings.MerchantMsisdn = model.MerchantMsisdn;
        syriatelPaymentSettings.AdditionalFee = model.AdditionalFee;
        syriatelPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

        await _settingService.SaveSettingAsync(syriatelPaymentSettings, x => x.BaseUrl, clearCache: false);
        await _settingService.SaveSettingAsync(syriatelPaymentSettings, x => x.Username, clearCache: false);
        await _settingService.SaveSettingAsync(syriatelPaymentSettings, x => x.Password, clearCache: false);
        await _settingService.SaveSettingAsync(syriatelPaymentSettings, x => x.MerchantMsisdn, clearCache: false);
        await _settingService.SaveSettingAsync(syriatelPaymentSettings, x => x.AdditionalFee, clearCache: false);
        await _settingService.SaveSettingAsync(syriatelPaymentSettings, x => x.AdditionalFeePercentage, clearCache: false);

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion
}
