using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nop.Core.Domain.Common;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Misc.Core.Controllers;

[Area("Admin")]
[AutoValidateAntiforgeryToken]
[ValidateIpAddress]
[AuthorizeAdmin(false)]
[ValidateVendor(false)]
[SaveSelectedTab(false, true)]
[NotNullValidationMessage]
public class NopStationAdminController : BaseController
{
	public override JsonResult Json(object data)
	{
		bool num = NopInstance.Load<AdminAreaSettings>()?.UseIsoDateFormatInJsonResult ?? false;
		JsonSerializerSettings jsonSerializerSettings = NopInstance.Load<IOptions<MvcNewtonsoftJsonOptions>>()?.Value?.SerializerSettings ?? new JsonSerializerSettings();
		if (!num)
		{
			return base.Json(data, jsonSerializerSettings);
		}
		jsonSerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
		jsonSerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
		return base.Json(data, jsonSerializerSettings);
	}
}
