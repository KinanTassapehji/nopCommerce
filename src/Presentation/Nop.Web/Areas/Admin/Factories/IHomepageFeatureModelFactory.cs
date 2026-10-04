using Nop.Core.Domain.Common;
using Nop.Web.Areas.Admin.Models.Common;

namespace Nop.Web.Areas.Admin.Factories;

/// <summary>
/// Represents the home page feature model factory
/// </summary>
public partial interface IHomepageFeatureModelFactory
{
    /// <summary>
    /// Prepare home page feature search model
    /// </summary>
    /// <param name="searchModel">Home page feature search model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the search model</returns>
    Task<HomepageFeatureSearchModel> PrepareHomepageFeatureSearchModelAsync(HomepageFeatureSearchModel searchModel);

    /// <summary>
    /// Prepare paged home page feature list model
    /// </summary>
    /// <param name="searchModel">Home page feature search model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the list model</returns>
    Task<HomepageFeatureListModel> PrepareHomepageFeatureListModelAsync(HomepageFeatureSearchModel searchModel);

    /// <summary>
    /// Prepare home page feature model
    /// </summary>
    /// <param name="model">Home page feature model</param>
    /// <param name="homepageFeature">Home page feature</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the model</returns>
    Task<HomepageFeatureModel> PrepareHomepageFeatureModelAsync(HomepageFeatureModel model,
        HomepageFeature homepageFeature, bool excludeProperties = false);
}
