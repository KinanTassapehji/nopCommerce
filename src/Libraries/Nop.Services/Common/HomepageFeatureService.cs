using Nop.Core.Domain.Common;
using Nop.Data;

namespace Nop.Services.Common;

/// <summary>
/// Home page trust strip items service
/// </summary>
public partial class HomepageFeatureService : IHomepageFeatureService
{
    #region Fields

    protected readonly IRepository<HomepageFeature> _homepageFeatureRepository;

    #endregion

    #region Ctor

    public HomepageFeatureService(IRepository<HomepageFeature> homepageFeatureRepository)
    {
        _homepageFeatureRepository = homepageFeatureRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the items, in display order
    /// </summary>
    /// <param name="showHidden">Whether to include unpublished items</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the items</returns>
    public virtual async Task<IList<HomepageFeature>> GetAllHomepageFeaturesAsync(bool showHidden = false)
    {
        //cached as one list (the home page asks on every visit); filtered in memory, it is a handful of rows
        var all = await _homepageFeatureRepository.GetAllAsync(query =>
        {
            return from feature in query
                orderby feature.DisplayOrder, feature.Id
                select feature;
        }, cache => default);

        return showHidden ? all : all.Where(feature => feature.Published).ToList();
    }

    /// <summary>
    /// Gets an item
    /// </summary>
    /// <param name="homepageFeatureId">Item identifier</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the item</returns>
    public virtual async Task<HomepageFeature> GetHomepageFeatureByIdAsync(int homepageFeatureId)
    {
        return await _homepageFeatureRepository.GetByIdAsync(homepageFeatureId, cache => default);
    }

    /// <summary>
    /// Inserts an item
    /// </summary>
    /// <param name="homepageFeature">Item</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertHomepageFeatureAsync(HomepageFeature homepageFeature)
    {
        await _homepageFeatureRepository.InsertAsync(homepageFeature);
    }

    /// <summary>
    /// Updates an item
    /// </summary>
    /// <param name="homepageFeature">Item</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateHomepageFeatureAsync(HomepageFeature homepageFeature)
    {
        await _homepageFeatureRepository.UpdateAsync(homepageFeature);
    }

    /// <summary>
    /// Deletes an item
    /// </summary>
    /// <param name="homepageFeature">Item</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteHomepageFeatureAsync(HomepageFeature homepageFeature)
    {
        await _homepageFeatureRepository.DeleteAsync(homepageFeature);
    }

    #endregion
}
