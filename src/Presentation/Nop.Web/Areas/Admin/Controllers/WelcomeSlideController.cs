using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers;

/// <summary>
/// Content management > Welcome slides: the tour a visitor sees on the first visit
/// </summary>
public partial class WelcomeSlideController : BaseAdminController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly ILocalizedEntityService _localizedEntityService;
    protected readonly INotificationService _notificationService;
    protected readonly IWelcomeSlideModelFactory _welcomeSlideModelFactory;
    protected readonly IWelcomeSlideService _welcomeSlideService;

    #endregion

    #region Ctor

    public WelcomeSlideController(ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        INotificationService notificationService,
        IWelcomeSlideModelFactory welcomeSlideModelFactory,
        IWelcomeSlideService welcomeSlideService)
    {
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _notificationService = notificationService;
        _welcomeSlideModelFactory = welcomeSlideModelFactory;
        _welcomeSlideService = welcomeSlideService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(WelcomeSlide welcomeSlide, WelcomeSlideModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(welcomeSlide, x => x.Title, localized.Title, localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(welcomeSlide, x => x.Text, localized.Text, localized.LanguageId);
        }
    }

    #endregion

    #region Methods

    public virtual IActionResult Index()
    {
        return RedirectToAction("List");
    }

    [CheckPermission(StandardPermission.ContentManagement.TOPICS_VIEW)]
    public virtual async Task<IActionResult> List()
    {
        //prepare model
        var model = await _welcomeSlideModelFactory.PrepareWelcomeSlideSearchModelAsync(new WelcomeSlideSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.ContentManagement.TOPICS_VIEW)]
    public virtual async Task<IActionResult> List(WelcomeSlideSearchModel searchModel)
    {
        //prepare model
        var model = await _welcomeSlideModelFactory.PrepareWelcomeSlideListModelAsync(searchModel);

        return Json(model);
    }

    [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create()
    {
        //prepare model
        var model = await _welcomeSlideModelFactory.PrepareWelcomeSlideModelAsync(new WelcomeSlideModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create(WelcomeSlideModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var welcomeSlide = model.ToEntity<WelcomeSlide>();
            await _welcomeSlideService.InsertWelcomeSlideAsync(welcomeSlide);

            //locales
            await UpdateLocalesAsync(welcomeSlide, model);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.WelcomeSlides.Added"));

            return continueEditing
                ? RedirectToAction("Edit", new { id = welcomeSlide.Id })
                : RedirectToAction("List");
        }

        //prepare model
        model = await _welcomeSlideModelFactory.PrepareWelcomeSlideModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [CheckPermission(StandardPermission.ContentManagement.TOPICS_VIEW)]
    public virtual async Task<IActionResult> Edit(int id)
    {
        //try to get a slide with the specified id
        var welcomeSlide = await _welcomeSlideService.GetWelcomeSlideByIdAsync(id);
        if (welcomeSlide == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _welcomeSlideModelFactory.PrepareWelcomeSlideModelAsync(null, welcomeSlide);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Edit(WelcomeSlideModel model, bool continueEditing)
    {
        //try to get a slide with the specified id
        var welcomeSlide = await _welcomeSlideService.GetWelcomeSlideByIdAsync(model.Id);
        if (welcomeSlide == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            welcomeSlide = model.ToEntity(welcomeSlide);
            await _welcomeSlideService.UpdateWelcomeSlideAsync(welcomeSlide);

            //locales
            await UpdateLocalesAsync(welcomeSlide, model);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.WelcomeSlides.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = welcomeSlide.Id });
        }

        //prepare model
        model = await _welcomeSlideModelFactory.PrepareWelcomeSlideModelAsync(model, welcomeSlide, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        //try to get a slide with the specified id
        var welcomeSlide = await _welcomeSlideService.GetWelcomeSlideByIdAsync(id)
            ?? throw new ArgumentException("No welcome slide found with the specified id", nameof(id));

        await _welcomeSlideService.DeleteWelcomeSlideAsync(welcomeSlide);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.WelcomeSlides.Deleted"));

        return RedirectToAction("List");
    }

    #endregion
}