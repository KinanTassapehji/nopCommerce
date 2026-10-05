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
/// Content management > Home page features: the trust strip under the home page slider
/// </summary>
public partial class HomepageFeatureController : BaseAdminController
{
    #region Fields

    protected readonly IHomepageFeatureModelFactory _homepageFeatureModelFactory;
    protected readonly IHomepageFeatureService _homepageFeatureService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILocalizedEntityService _localizedEntityService;
    protected readonly INotificationService _notificationService;

    #endregion

    #region Ctor

    public HomepageFeatureController(IHomepageFeatureModelFactory homepageFeatureModelFactory,
        IHomepageFeatureService homepageFeatureService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        INotificationService notificationService)
    {
        _homepageFeatureModelFactory = homepageFeatureModelFactory;
        _homepageFeatureService = homepageFeatureService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _notificationService = notificationService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(HomepageFeature homepageFeature, HomepageFeatureModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(homepageFeature, x => x.Title, localized.Title, localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(homepageFeature, x => x.Hint, localized.Hint, localized.LanguageId);
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
        var model = await _homepageFeatureModelFactory.PrepareHomepageFeatureSearchModelAsync(new HomepageFeatureSearchModel());

        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.ContentManagement.TOPICS_VIEW)]
    public virtual async Task<IActionResult> List(HomepageFeatureSearchModel searchModel)
    {
        //prepare model
        var model = await _homepageFeatureModelFactory.PrepareHomepageFeatureListModelAsync(searchModel);

        return Json(model);
    }

    [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create()
    {
        //prepare model
        var model = await _homepageFeatureModelFactory.PrepareHomepageFeatureModelAsync(new HomepageFeatureModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create(HomepageFeatureModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var homepageFeature = model.ToEntity<HomepageFeature>();
            await _homepageFeatureService.InsertHomepageFeatureAsync(homepageFeature);

            //locales
            await UpdateLocalesAsync(homepageFeature, model);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.HomepageFeatures.Added"));

            return continueEditing
                ? RedirectToAction("Edit", new { id = homepageFeature.Id })
                : RedirectToAction("List");
        }

        //prepare model
        model = await _homepageFeatureModelFactory.PrepareHomepageFeatureModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [CheckPermission(StandardPermission.ContentManagement.TOPICS_VIEW)]
    public virtual async Task<IActionResult> Edit(int id)
    {
        //try to get an item with the specified id
        var homepageFeature = await _homepageFeatureService.GetHomepageFeatureByIdAsync(id);
        if (homepageFeature == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _homepageFeatureModelFactory.PrepareHomepageFeatureModelAsync(null, homepageFeature);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Edit(HomepageFeatureModel model, bool continueEditing)
    {
        //try to get an item with the specified id
        var homepageFeature = await _homepageFeatureService.GetHomepageFeatureByIdAsync(model.Id);
        if (homepageFeature == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            homepageFeature = model.ToEntity(homepageFeature);
            await _homepageFeatureService.UpdateHomepageFeatureAsync(homepageFeature);

            //locales
            await UpdateLocalesAsync(homepageFeature, model);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.HomepageFeatures.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = homepageFeature.Id });
        }

        //prepare model
        model = await _homepageFeatureModelFactory.PrepareHomepageFeatureModelAsync(model, homepageFeature, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        //try to get an item with the specified id
        var homepageFeature = await _homepageFeatureService.GetHomepageFeatureByIdAsync(id)
            ?? throw new ArgumentException("No home page feature found with the specified id", nameof(id));

        await _homepageFeatureService.DeleteHomepageFeatureAsync(homepageFeature);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.HomepageFeatures.Deleted"));

        return RedirectToAction("List");
    }

    #endregion
}
