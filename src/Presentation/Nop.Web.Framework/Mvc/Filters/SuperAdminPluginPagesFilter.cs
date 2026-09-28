using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Data;
using Nop.Services.Security;

namespace Nop.Web.Framework.Mvc.Filters;

/// <summary>
/// Everything left in the Nop Station admin section is super-administrator only: every NopStation plugin's settings
/// page, customer reminders, the mega menu with its category icons, and NopStation's core settings, ACL, licence
/// and string resources (the last listed under Configuration). Its controllers ship compiled, so they cannot carry
/// a [CheckPermission]; this global filter guards them by route. The slider, carousel and product tab lists moved
/// to Content management stay open to administrators (other actions on those controllers).
/// </summary>
public partial class SuperAdminPluginPagesFilter : IAsyncAuthorizationFilter
{
    #region Fields

    //NopStation controllers guarded as a whole
    protected static readonly string[] _guardedControllers =
    [
        "NopStationCore", "NopStationLicense", "CustomerReminders", "ReminderRule", "Reminder", "ReminderReport", "MegaMenu", "CategoryIcon"
    ];

    protected readonly IPermissionService _permissionService;
    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public SuperAdminPluginPagesFilter(IPermissionService permissionService, IWebHelper webHelper)
    {
        _permissionService = permissionService;
        _webHelper = webHelper;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Whether the route is one of the guarded NopStation pages
    /// </summary>
    protected virtual bool IsGuarded(ActionDescriptor actionDescriptor, string controller, string action)
    {
        if (actionDescriptor is not ControllerActionDescriptor descriptor
            || descriptor.ControllerTypeInfo.Assembly.GetName().Name?.StartsWith("NopStation.", StringComparison.OrdinalIgnoreCase) != true)
            return false;

        if (_guardedControllers.Contains(controller, StringComparer.OrdinalIgnoreCase))
            return true;

        //every plugin's settings page; its list and edit pages (sliders, carousels, product tabs) stay open
        return string.Equals(action, "Configure", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Called early in the filter pipeline to confirm request is authorized
    /// </summary>
    /// <param name="context">Authorization filter context</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var controller = context.RouteData.Values["controller"] as string;
        var action = context.RouteData.Values["action"] as string;
        if (!IsGuarded(context.ActionDescriptor, controller, action))
            return;

        if (await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGIN_AND_WIDGET_LISTS))
            return;

        var request = context.HttpContext.Request;
        context.Result = HttpMethods.IsGet(request.Method)
            ? new RedirectToActionResult("AccessDenied", "Security",
                new { area = AreaNames.ADMIN, pageUrl = _webHelper.GetRawUrl(request), pageSystemNameKey = $"{controller}.{action}" })
            : new StatusCodeResult(StatusCodes.Status403Forbidden);
    }

    #endregion
}