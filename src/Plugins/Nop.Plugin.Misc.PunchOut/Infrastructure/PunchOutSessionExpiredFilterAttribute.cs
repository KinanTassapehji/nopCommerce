using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Http;
using Nop.Plugin.Misc.PunchOut.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.PunchOut.Infrastructure;

/// <summary>
/// Represents filter attribute to check if PunchOut session is expired
/// </summary>
public class PunchOutSessionExpiredFilterAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    public PunchOutSessionExpiredFilterAttribute() : base(typeof(PunchOutSessionExpiredFilter))
    {
    }

    #endregion

    #region Nested filter

    /// <summary>
    /// Represents filter to check if PunchOut session is expired
    /// </summary>
    private class PunchOutSessionExpiredFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly PunchOutService _punchOutService;
        private readonly PunchOutSettings _punchOutSettings;

        #endregion

        #region Ctor

        public PunchOutSessionExpiredFilter(ILocalizationService localizationService,
            INotificationService notificationService,
            PunchOutService punchOutService,
            PunchOutSettings punchOutSettings)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _punchOutService = punchOutService;
            _punchOutSettings = punchOutSettings;
        }

        #endregion

        #region Utilities

        private async Task IsActivePunchoutSession(ActionExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!_punchOutSettings.IsActive)
                return;

            var session = await _punchOutService.GetPunchOutSessionAsync();
            if (session != null && session.IsActive)
            {
                var timeToExpire = _punchOutSettings.TimeToExpire;
                if (session.CreatedOnUtc.AddHours(timeToExpire) < DateTime.UtcNow)
                {
                    await _punchOutService.ClearPunchoutSessionDataAsync();

                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Misc.PunchOut.SessionExpired"));
                    context.Result = new RedirectToRouteResult(NopRouteNames.General.HOMEPAGE, null);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called asynchronously before the action, after model binding is complete.
        /// </summary>
        /// <param name="context">A context for action filters</param>
        /// <param name="next">A delegate invoked to execute the next action filter or the action itself</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await IsActivePunchoutSession(context);

            if (context.Result == null)
                await next();
        }

        #endregion
    }

    #endregion
}
