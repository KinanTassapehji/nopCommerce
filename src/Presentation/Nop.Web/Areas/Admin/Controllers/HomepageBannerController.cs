using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers;

/// <summary>
/// Content management > Home page banner: the call to action band on the home page
/// </summary>
//ponytail: one banner for all stores - per-store overrides when there is a second store
public partial class HomepageBannerController : BaseAdminController
{
    #region Fields

    protected readonly ILanguageService _languageService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public HomepageBannerController(ILanguageService languageService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService)
    {
        _languageService = languageService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public virtual IActionResult Index()
    {
        return RedirectToAction("Configure");
    }

    [CheckPermission(StandardPermission.ContentManagement.TOPICS_VIEW)]
    public virtual async Task<IActionResult> Configure()
    {
        var settings = await _settingService.LoadSettingAsync<HomepageBannerSettings>();
        var model = new HomepageBannerModel
        {
            Enabled = settings.Enabled,
            PictureId = settings.PictureId,
            Title = settings.Title,
            Text = settings.Text,
            ButtonText = settings.ButtonText,
            ButtonUrl = settings.ButtonUrl
        };
        await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
        {
            locale.Title = await _localizationService.GetLocalizedSettingAsync(settings, x => x.Title, languageId, 0, false, false);
            locale.Text = await _localizationService.GetLocalizedSettingAsync(settings, x => x.Text, languageId, 0, false, false);
            locale.ButtonText = await _localizationService.GetLocalizedSettingAsync(settings, x => x.ButtonText, languageId, 0, false, false);
        });

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Configure(HomepageBannerModel model)
    {
        var settings = await _settingService.LoadSettingAsync<HomepageBannerSettings>();
        settings.Enabled = model.Enabled;
        settings.PictureId = model.PictureId;
        settings.Title = model.Title;
        settings.Text = model.Text;
        settings.ButtonText = model.ButtonText;
        settings.ButtonUrl = model.ButtonUrl?.Trim();
        await _settingService.SaveSettingAsync(settings);

        //localized values hang off the setting rows saved above
        foreach (var localized in model.Locales)
        {
            await _localizationService.SaveLocalizedSettingAsync(settings, x => x.Title, localized.LanguageId, localized.Title);
            await _localizationService.SaveLocalizedSettingAsync(settings, x => x.Text, localized.LanguageId, localized.Text);
            await _localizationService.SaveLocalizedSettingAsync(settings, x => x.ButtonText, localized.LanguageId, localized.ButtonText);
        }

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion
}