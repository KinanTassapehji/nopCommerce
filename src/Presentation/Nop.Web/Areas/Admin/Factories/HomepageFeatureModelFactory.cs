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
/// Represents the home page feature model factory implementation
/// </summary>
public partial class HomepageFeatureModelFactory : IHomepageFeatureModelFactory
{
    #region Fields

    protected readonly IHomepageFeatureService _homepageFeatureService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILocalizedModelFactory _localizedModelFactory;

    #endregion

    #region Ctor

    public HomepageFeatureModelFactory(IHomepageFeatureService homepageFeatureService,
        ILocalizationService localizationService,
        ILocalizedModelFactory localizedModelFactory)
    {
        _homepageFeatureService = homepageFeatureService;
        _localizationService = localizationService;
        _localizedModelFactory = localizedModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare home page feature search model
    /// </summary>
    /// <param name="searchModel">Home page feature search model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the search model</returns>
    public virtual Task<HomepageFeatureSearchModel> PrepareHomepageFeatureSearchModelAsync(HomepageFeatureSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare page parameters
        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    /// <summary>
    /// Prepare paged home page feature list model
    /// </summary>
    /// <param name="searchModel">Home page feature search model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the list model</returns>
    public virtual async Task<HomepageFeatureListModel> PrepareHomepageFeatureListModelAsync(HomepageFeatureSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get items, unpublished included
        var features = (await _homepageFeatureService.GetAllHomepageFeaturesAsync(showHidden: true)).ToPagedList(searchModel);

        //prepare list model
        var model = await new HomepageFeatureListModel().PrepareToGridAsync(searchModel, features, () =>
        {
            return features.SelectAwait(async feature =>
            {
                var featureModel = feature.ToModel<HomepageFeatureModel>();
                featureModel.IconName = await _localizationService.GetLocalizedEnumAsync(feature.Icon);

                return featureModel;
            });
        });

        return model;
    }

    /// <summary>
    /// Prepare home page feature model
    /// </summary>
    /// <param name="model">Home page feature model</param>
    /// <param name="homepageFeature">Home page feature</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the model</returns>
    public virtual async Task<HomepageFeatureModel> PrepareHomepageFeatureModelAsync(HomepageFeatureModel model,
        HomepageFeature homepageFeature, bool excludeProperties = false)
    {
        Func<HomepageFeatureLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (homepageFeature != null)
        {
            //fill in model values from the entity
            model ??= homepageFeature.ToModel<HomepageFeatureModel>();

            //define localized model configuration action
            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Title = await _localizationService.GetLocalizedAsync(homepageFeature, entity => entity.Title, languageId, false, false);
                locale.Hint = await _localizationService.GetLocalizedAsync(homepageFeature, entity => entity.Hint, languageId, false, false);
            };
        }
        else if (!excludeProperties)
        {
            //defaults for a new item: shown, after the existing ones
            var features = await _homepageFeatureService.GetAllHomepageFeaturesAsync(showHidden: true);
            model.Published = true;
            model.IconId = (int)HomepageFeatureIcon.Star;
            model.DisplayOrder = features.Any() ? features.Max(feature => feature.DisplayOrder) + 1 : 1;
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
