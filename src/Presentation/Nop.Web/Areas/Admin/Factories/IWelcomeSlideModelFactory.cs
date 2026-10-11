using Nop.Core.Domain.Common;
using Nop.Web.Areas.Admin.Models.Common;

namespace Nop.Web.Areas.Admin.Factories;

/// <summary>
/// Represents the welcome slide model factory
/// </summary>
public partial interface IWelcomeSlideModelFactory
{
    /// <summary>
    /// Prepare welcome slide search model
    /// </summary>
    /// <param name="searchModel">Welcome slide search model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the search model</returns>
    Task<WelcomeSlideSearchModel> PrepareWelcomeSlideSearchModelAsync(WelcomeSlideSearchModel searchModel);

    /// <summary>
    /// Prepare paged welcome slide list model
    /// </summary>
    /// <param name="searchModel">Welcome slide search model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the list model</returns>
    Task<WelcomeSlideListModel> PrepareWelcomeSlideListModelAsync(WelcomeSlideSearchModel searchModel);

    /// <summary>
    /// Prepare welcome slide model
    /// </summary>
    /// <param name="model">Welcome slide model</param>
    /// <param name="welcomeSlide">Welcome slide</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>A task that represents the asynchronous operation; the task result contains the model</returns>
    Task<WelcomeSlideModel> PrepareWelcomeSlideModelAsync(WelcomeSlideModel model,
        WelcomeSlide welcomeSlide, bool excludeProperties = false);
}