using Nop.Core.Domain.Common;

namespace Nop.Services.Common;

/// <summary>
/// Welcome tour slides service
/// </summary>
public partial interface IWelcomeSlideService
{
    /// <summary>
    /// Gets the slides, in display order
    /// </summary>
    /// <param name="showHidden">Whether to include unpublished slides</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the slides</returns>
    Task<IList<WelcomeSlide>> GetAllWelcomeSlidesAsync(bool showHidden = false);

    /// <summary>
    /// Gets a slide
    /// </summary>
    /// <param name="welcomeSlideId">Slide identifier</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the slide</returns>
    Task<WelcomeSlide> GetWelcomeSlideByIdAsync(int welcomeSlideId);

    /// <summary>
    /// Inserts a slide
    /// </summary>
    /// <param name="welcomeSlide">Slide</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertWelcomeSlideAsync(WelcomeSlide welcomeSlide);

    /// <summary>
    /// Updates a slide
    /// </summary>
    /// <param name="welcomeSlide">Slide</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateWelcomeSlideAsync(WelcomeSlide welcomeSlide);

    /// <summary>
    /// Deletes a slide
    /// </summary>
    /// <param name="welcomeSlide">Slide</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteWelcomeSlideAsync(WelcomeSlide welcomeSlide);
}