using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Data;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.Core.Filters;

public class EditAccessAttribute : TypeFilterAttribute
{
	private class EditAccessFilter : IAuthorizationFilter, IFilterMetadata
	{
		private readonly bool _ignoreFilter;

		private readonly IPermissionService _permissionService;

		public EditAccessFilter(bool ignoreFilter, IPermissionService permissionService)
		{
			_ignoreFilter = ignoreFilter;
			_permissionService = permissionService;
		}

		public void OnAuthorization(AuthorizationFilterContext filterContext)
		{
			ArgumentNullException.ThrowIfNull(filterContext, "filterContext");
			if (!((from filterDescriptor in filterContext.ActionDescriptor.FilterDescriptors
				where filterDescriptor.Scope == FilterScope.Action
				select filterDescriptor.Filter).OfType<EditAccessAttribute>().FirstOrDefault()?.IgnoreFilter ?? _ignoreFilter) && DataSettingsManager.IsDatabaseInstalled() && !_permissionService.AuthorizeAsync("ManageNopStationFeatures").Result)
			{
				string returnUrl = "/";
				if (filterContext.HttpContext?.Request?.Headers?.ContainsKey("Referer") == true)
				{
					returnUrl = filterContext.HttpContext.Request.Headers["Referer"].ToString();
				}
				filterContext.Result = new RedirectToActionResult("EditAccessRedirect", "NopStation", new { returnUrl });
			}
		}
	}

	private readonly bool _ignoreFilter;

	public bool IgnoreFilter => _ignoreFilter;

	public EditAccessAttribute(bool ignore = false)
		: base(typeof(EditAccessFilter))
	{
		_ignoreFilter = ignore;
		base.Arguments = new object[1] { ignore };
	}
}
