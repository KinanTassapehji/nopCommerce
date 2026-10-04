using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers;

/// <summary>
/// Phone verification: link the store's WhatsApp number (scan its QR) and switch the codes on
/// </summary>
public partial class PhoneVerificationController : BaseAdminController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly ISettingService _settingService;
    protected readonly PhoneVerificationSettings _phoneVerificationSettings;
    protected readonly WhatsAppSidecarClient _whatsAppSidecarClient;

    #endregion

    #region Ctor

    public PhoneVerificationController(ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        PhoneVerificationSettings phoneVerificationSettings,
        WhatsAppSidecarClient whatsAppSidecarClient)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _phoneVerificationSettings = phoneVerificationSettings;
        _whatsAppSidecarClient = whatsAppSidecarClient;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Run a request to the sidecar, saying so when it does not answer
    /// </summary>
    protected virtual async Task<IActionResult> CallSidecarAsync(Func<Task> request)
    {
        try
        {
            await request();
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or UriFormatException)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.PhoneVerification.SidecarOffline"));
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_SETTINGS)]
    public virtual async Task<IActionResult> Index()
    {
        var model = new PhoneVerificationSettingsModel
        {
            Enabled = _phoneVerificationSettings.Enabled,
            WhatsAppSidecarUrl = _phoneVerificationSettings.WhatsAppSidecarUrl
        };
        if (!string.IsNullOrEmpty(model.WhatsAppSidecarUrl))
            model.Accounts = await _whatsAppSidecarClient.GetAccountsAsync();

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_SETTINGS)]
    public virtual async Task<IActionResult> Index(PhoneVerificationSettingsModel model)
    {
        _phoneVerificationSettings.Enabled = model.Enabled;
        _phoneVerificationSettings.WhatsAppSidecarUrl = model.WhatsAppSidecarUrl?.Trim();
        await _settingService.SaveSettingAsync(_phoneVerificationSettings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_SETTINGS)]
    public virtual Task<IActionResult> AddNumber()
    {
        return CallSidecarAsync(() => _whatsAppSidecarClient.AddAccountAsync());
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_SETTINGS)]
    public virtual Task<IActionResult> Unlink(string id)
    {
        return CallSidecarAsync(() => _whatsAppSidecarClient.LogoutAccountAsync(id));
    }

    #endregion
}