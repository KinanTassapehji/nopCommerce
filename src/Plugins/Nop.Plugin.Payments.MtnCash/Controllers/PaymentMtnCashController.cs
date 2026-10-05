using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.MtnCash.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.MtnCash.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class PaymentMtnCashController : BasePaymentController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;


    #endregion

    #region Ctor

    public PaymentMtnCashController(ILocalizationService localizationService,
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
        var MtnCashPaymentSettings = await _settingService.LoadSettingAsync<MtnCashPaymentSettings>();

        var model = new ConfigurationModel
        {
            AdditionalFee = MtnCashPaymentSettings.AdditionalFee,
            AdditionalFeePercentage = MtnCashPaymentSettings.AdditionalFeePercentage,
        };

        return View("~/Plugins/Payments.MtnCash/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();
        var MtnCashPaymentSettings = await _settingService.LoadSettingAsync<MtnCashPaymentSettings>();

        //save settings
        MtnCashPaymentSettings.AdditionalFee = model.AdditionalFee;
        MtnCashPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;


        await _settingService.SaveSettingAsync(MtnCashPaymentSettings, x => x.AdditionalFee, clearCache: false);
        await _settingService.SaveSettingAsync(MtnCashPaymentSettings, x => x.AdditionalFeePercentage, clearCache: false);

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion
}