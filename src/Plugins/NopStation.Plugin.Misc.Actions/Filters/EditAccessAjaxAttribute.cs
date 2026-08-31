using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.Core.Filters;

public class EditAccessAjaxAttribute : TypeFilterAttribute
{
	private class EditAccessAjaxFilter : IAuthorizationFilter, IFilterMetadata
	{
		private readonly bool _ignoreFilter;

		private readonly IPermissionService _permissionService;

		private readonly ILocalizationService _localizationService;

		public EditAccessAjaxFilter(bool ignoreFilter, IPermissionService permissionService, ILocalizationService localizationService)
		{
			_ignoreFilter = ignoreFilter;
			_permissionService = permissionService;
			_localizationService = localizationService;
		}

		public void OnAuthorization(AuthorizationFilterContext filterContext)
		{
			ArgumentNullException.ThrowIfNull(filterContext, "filterContext");
			if (!((from filterDescriptor in filterContext.ActionDescriptor.FilterDescriptors
				where filterDescriptor.Scope == FilterScope.Action
				select filterDescriptor.Filter).OfType<EditAccessAjaxAttribute>().FirstOrDefault()?.IgnoreFilter ?? _ignoreFilter) && DataSettingsManager.IsDatabaseInstalled() && !_permissionService.AuthorizeAsync("ManageNopStationFeatures").Result)
			{
				string result = _localizationService.GetResourceAsync("Admin.NopStation.Core.Resources.EditAccessDenied").Result;
				filterContext.Result = new JsonResult(new
				{
					error = result,
					Error = result,
					Message = result,
					Result = false
				});
			}
		}
	}

	private readonly bool _ignoreFilter;

	public bool IgnoreFilter => _ignoreFilter;

	public EditAccessAjaxAttribute(bool ignore = false)
		: base(typeof(EditAccessAjaxFilter))
	{
		_ignoreFilter = ignore;
		base.Arguments = new object[1] { ignore };
	}
}
