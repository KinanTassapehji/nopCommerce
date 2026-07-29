using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.PunchOut.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.PunchOut.Components;

/// <summary>
/// Represents the view component to display additional buttons in the shopping cart
/// </summary>
public class PunchOutButtonComponent : NopViewComponent
{
    #region Fields

    private readonly PunchOutService _punchOutService;
    private readonly PunchOutSettings _punchOutSettings;

    #endregion

    #region Ctor

    public PunchOutButtonComponent(PunchOutService punchOutService,
        PunchOutSettings punchOutSettings)
    {
        _punchOutService = punchOutService;
        _punchOutSettings = punchOutSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Invoke view component
    /// </summary>
    /// <param name="widgetZone">Widget zone name</param>
    /// <param name="additionalData">Additional data</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the view component result
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!_punchOutSettings.IsActive || !await _punchOutService.IsPunchoutSessionAsync())
            return Content(string.Empty);

        return await ViewAsync("~/Plugins/Misc.PunchOut/Views/Components/PunchOutButton.cshtml");
    }

    #endregion
}
