using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Customers;
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
        private readonly IWorkContext _workContext;
        private readonly PunchOutService _punchOutService;
        private readonly PunchOutSettings _punchOutSettings;

        /// <summary>
        /// Controllers that are allowed during active PunchOut session
        /// </summary>
        private static readonly HashSet<string> _allowedControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Home",
            "Catalog",
            "Product",
            "PunchOut",
            "ShoppingCart",
            "Error",
            "Common"
        };

        /// <summary>
        /// Specific actions that are forbidden during active PunchOut session
        /// Key: Controller name, Value: Set of forbidden action names
        /// </summary>
        private static readonly Dictionary<string, HashSet<string>> _forbiddenActions = new(StringComparer.OrdinalIgnoreCase)
        {
            {
                "ShoppingCart",
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "StartCheckout",
                    "Checkout"
                }
            }
        };

        #endregion

        #region Ctor

        public PunchOutSessionExpiredFilter(ILocalizationService localizationService,
            INotificationService notificationService,
            IWorkContext workContext,
            PunchOutService punchOutService,
            PunchOutSettings punchOutSettings)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _workContext = workContext;
            _punchOutService = punchOutService;
            _punchOutSettings = punchOutSettings;
        }

        #endregion

        #region Utilities

        private async Task IsActivePunchoutSession(ActionExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var actionName = actionDescriptor?.ActionName;
            var controllerName = actionDescriptor?.ControllerName;
            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                return;

            if (!_punchOutSettings.IsActive)
                return;

            var customer = await _workContext.GetCurrentCustomerAsync();

            //ignore search engines and background tasks
            if (customer.IsSearchEngineAccount() || customer.IsBackgroundTaskAccount())
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
                else if (!_allowedControllers.Contains(controllerName))
                {
                    //if PunchOut session is active, restrict access to all controllers except those in AllowedControllers
                    context.Result = new RedirectToRouteResult(NopRouteNames.General.HOMEPAGE, null);
                }
                else if (IsForbiddenAction(controllerName, actionName))
                {
                    //if specific action is forbidden during active PunchOut session, restrict access
                    context.Result = new RedirectToRouteResult(NopRouteNames.General.HOMEPAGE, null);
                }
            }
        }

        /// <summary>
        /// Checks if the action is forbidden during active PunchOut session
        /// </summary>
        /// <param name="controllerName">The name of the controller</param>
        /// <param name="actionName">The name of the action</param>
        /// <returns>True if the action is forbidden; otherwise false</returns>
        private bool IsForbiddenAction(string controllerName, string actionName)
        {
            return _forbiddenActions.TryGetValue(controllerName, out var forbiddenActions) &&
                   forbiddenActions.Contains(actionName);
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
