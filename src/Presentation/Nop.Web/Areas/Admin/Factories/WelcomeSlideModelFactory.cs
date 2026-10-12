using Nop.Core.Domain.Common;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories;

/// <summary>
/// Represents the welcome slide model factory implementation
/// </summary>
public partial class WelcomeSlideModelFactory : IWelcomeSlideModelFactory
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly ILocalizedModelFactory _localizedModelFactory;
    protected readonly IWelcomeSlideService _welcomeSlideService;

    #endregion

    #region Ctor

    public WelcomeSlideModelFactory(ILocalizationService localizationService,
        ILocalizedModelFactory localizedModelFactory,
        IWelcomeSlideService welcomeSlideService)
    {
        _localizationService = localizationService;
        _localizedModelFactory = localizedModelFactory;
        _welcomeSlideService = welcomeSlideService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare welcome slide search model
    /// </summary>
    /// <param name="searchModel">Welcome slide search model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the search model</returns>
    public virtual Task<WelcomeSlideSearchModel> PrepareWelcomeSlideSearchModelAsync(WelcomeSlideSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare page parameters
        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    /// <summary>
    /// Prepare paged welcome slide list model
    /// </summary>
    /// <param name="searchModel">Welcome slide search model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the list model</returns>
    public virtual async Task<WelcomeSlideListModel> PrepareWelcomeSlideListModelAsync(WelcomeSlideSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get slides, unpublished included
        var slides = (await _welcomeSlideService.GetAllWelcomeSlidesAsync(showHidden: true)).ToPagedList(searchModel);

        //prepare list model
        var model = await new WelcomeSlideListModel().PrepareToGridAsync(searchModel, slides, () =>
        {
            return slides.SelectAwait(async slide =>
            {
                var slideModel = slide.ToModel<WelcomeSlideModel>();
                slideModel.IconName = await _localizationService.GetLocalizedEnumAsync(slide.Icon);

                return slideModel;
            });
        });

        return model;
    }

    /// <summary>
    /// Prepare welcome slide model
    /// </summary>
    /// <param name="model">Welcome slide model</param>
    /// <param name="welcomeSlide">Welcome slide</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the model</returns>
    public virtual async Task<WelcomeSlideModel> PrepareWelcomeSlideModelAsync(WelcomeSlideModel model,
        WelcomeSlide welcomeSlide, bool excludeProperties = false)
    {
        Func<WelcomeSlideLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (welcomeSlide != null)
        {
            //fill in model values from the entity
            model ??= welcomeSlide.ToModel<WelcomeSlideModel>();

            //define localized model configuration action
            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Title = await _localizationService.GetLocalizedAsync(welcomeSlide, entity => entity.Title, languageId, false, false);
                locale.Text = await _localizationService.GetLocalizedAsync(welcomeSlide, entity => entity.Text, languageId, false, false);
            };
        }
        else if (!excludeProperties)
        {
            //defaults for a new slide: shown, after the existing ones
            var slides = await _welcomeSlideService.GetAllWelcomeSlidesAsync(showHidden: true);
            model.Published = true;
            model.IconId = (int)HomepageFeatureIcon.Star;
            model.DisplayOrder = slides.Any() ? slides.Max(slide => slide.DisplayOrder) + 1 : 1;
        }

        //the icon picker
        model.AvailableIcons = (await HomepageFeatureIcon.Delivery.ToSelectListAsync(false)).ToList();

        //prepare localized models
        if (!excludeProperties)
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        return model;
    }

    #endregion
}