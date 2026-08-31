using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Data;
using Nop.Services.Customers;

namespace NopStation.Plugin.Misc.Core.Filters;

public class CheckAccessAttribute : TypeFilterAttribute
{
	private class CheckAccessFilter : IAuthorizationFilter, IFilterMetadata
	{
		private readonly IWorkContext _workContext;

		private readonly NopStationCoreSettings _coreSettings;

		private readonly ICustomerService _customerService;

		private readonly IWebHelper _webHelper;

		private readonly IHttpContextAccessor _httpContextAccessor;

		public CheckAccessFilter(IWorkContext workContext, NopStationCoreSettings coreSettings, ICustomerService customerService, IWebHelper webHelper, IHttpContextAccessor httpContextAccessor)
		{
			_workContext = workContext;
			_coreSettings = coreSettings;
			_customerService = customerService;
			_webHelper = webHelper;
			_httpContextAccessor = httpContextAccessor;
		}

		public void OnAuthorization(AuthorizationFilterContext filterContext)
		{
			ArgumentNullException.ThrowIfNull(filterContext, "filterContext");
			if (!DataSettingsManager.IsDatabaseInstalled() || !_coreSettings.RestrictMainMenuByCustomerRoles)
			{
				return;
			}
			int[] result = _customerService.GetCustomerRoleIdsAsync(_workContext.GetCurrentCustomerAsync().Result).Result;
			foreach (int item in result)
			{
				if (_coreSettings.AllowedCustomerRoleIds.Contains(item))
				{
					return;
				}
			}
			filterContext.Result = new RedirectToActionResult("AccessDenied", "Security", new
			{
				pageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request)
			});
		}
	}

	public CheckAccessAttribute()
		: base(typeof(CheckAccessFilter))
	{
	}
}
