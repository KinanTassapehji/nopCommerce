using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Data;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Filters;

public class NopStationApiLicenseAttribute : TypeFilterAttribute
{
	private class NopStationApiLicenseFilter : IAuthorizationFilter, IFilterMetadata
	{
		private readonly ILicenseService _licenseService;

		public NopStationApiLicenseFilter(ILicenseService licenseService)
		{
			_licenseService = licenseService;
		}

		public void OnAuthorization(AuthorizationFilterContext filterContext)
		{
			ArgumentNullException.ThrowIfNull(filterContext, "filterContext");
			if (DataSettingsManager.IsDatabaseInstalled())
			{
				ControllerActionDescriptor controllerActionDescriptor = filterContext?.ActionDescriptor as ControllerActionDescriptor;
				if (!_licenseService.IsLicensedAsync(controllerActionDescriptor.ControllerTypeInfo.Assembly).Result)
				{
					CreateNstAccessResponceMessage(filterContext);
				}
			}
		}

		private void CreateNstAccessResponceMessage(AuthorizationFilterContext filterContext)
		{
			ILocalizationService localizationService = NopInstance.Load<ILocalizationService>();
			BaseResponseModel error = new BaseResponseModel
			{
				ErrorList = new List<string> { localizationService.GetResourceAsync("NopStation.WebApi.Response.InvalidLicense").Result }
			};
			filterContext.Result = new BadRequestObjectResult(error);
		}
	}

	public class BaseResponseModel
	{
		public List<string> ErrorList { get; set; }

		public BaseResponseModel()
		{
			ErrorList = new List<string>();
		}
	}

	public NopStationApiLicenseAttribute()
		: base(typeof(NopStationApiLicenseFilter))
	{
	}
}
