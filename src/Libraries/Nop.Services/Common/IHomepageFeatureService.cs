using Nop.Core.Domain.Common;

namespace Nop.Services.Common;

/// <summary>
/// Home page trust strip items service
/// </summary>
public partial interface IHomepageFeatureService
{
    /// <summary>
    /// Gets the items, in display order
    /// </summary>
    /// <param name="showHidden">Whether to include unpublished items</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the items</returns>
    Task<IList<HomepageFeature>> GetAllHomepageFeaturesAsync(bool showHidden = false);

    /// <summary>
    /// Gets an item
    /// </summary>
    /// <param name="homepageFeatureId">Item identifier</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the item</returns>
    Task<HomepageFeature> GetHomepageFeatureByIdAsync(int homepageFeatureId);

    /// <summary>
    /// Inserts an item
    /// </summary>
    /// <param name="homepageFeature">Item</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertHomepageFeatureAsync(HomepageFeature homepageFeature);

    /// <summary>
    /// Updates an item
    /// </summary>
    /// <param name="homepageFeature">Item</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateHomepageFeatureAsync(HomepageFeature homepageFeature);

    /// <summary>
    /// Deletes an item
    /// </summary>
    /// <param name="homepageFeature">Item</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteHomepageFeatureAsync(HomepageFeature homepageFeature);
}
