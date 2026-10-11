using Nop.Core.Domain.Common;
using Nop.Data;

namespace Nop.Services.Common;

/// <summary>
/// Welcome tour slides service
/// </summary>
public partial class WelcomeSlideService : IWelcomeSlideService
{
    #region Fields

    protected readonly IRepository<WelcomeSlide> _welcomeSlideRepository;

    #endregion

    #region Ctor

    public WelcomeSlideService(IRepository<WelcomeSlide> welcomeSlideRepository)
    {
        _welcomeSlideRepository = welcomeSlideRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the slides, in display order
    /// </summary>
    /// <param name="showHidden">Whether to include unpublished slides</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the slides</returns>
    public virtual async Task<IList<WelcomeSlide>> GetAllWelcomeSlidesAsync(bool showHidden = false)
    {
        //cached as one list (the home page asks on every visit); filtered in memory, it is a handful of rows
        var all = await _welcomeSlideRepository.GetAllAsync(query =>
        {
            return from slide in query
                orderby slide.DisplayOrder, slide.Id
                select slide;
        }, cache => default);

        return showHidden ? all : all.Where(slide => slide.Published).ToList();
    }

    /// <summary>
    /// Gets a slide
    /// </summary>
    /// <param name="welcomeSlideId">Slide identifier</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the slide</returns>
    public virtual async Task<WelcomeSlide> GetWelcomeSlideByIdAsync(int welcomeSlideId)
    {
        return await _welcomeSlideRepository.GetByIdAsync(welcomeSlideId, cache => default);
    }

    /// <summary>
    /// Inserts a slide
    /// </summary>
    /// <param name="welcomeSlide">Slide</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertWelcomeSlideAsync(WelcomeSlide welcomeSlide)
    {
        await _welcomeSlideRepository.InsertAsync(welcomeSlide);
    }

    /// <summary>
    /// Updates a slide
    /// </summary>
    /// <param name="welcomeSlide">Slide</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateWelcomeSlideAsync(WelcomeSlide welcomeSlide)
    {
        await _welcomeSlideRepository.UpdateAsync(welcomeSlide);
    }

    /// <summary>
    /// Deletes a slide
    /// </summary>
    /// <param name="welcomeSlide">Slide</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteWelcomeSlideAsync(WelcomeSlide welcomeSlide)
    {
        await _welcomeSlideRepository.DeleteAsync(welcomeSlide);
    }

    #endregion
}