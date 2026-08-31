using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace NopStation.Plugin.Misc.Core.Controllers;

[WwwRequirement]
[CheckLanguageSeoCode(false)]
[CheckAccessPublicStore(false)]
[CheckAccessClosedStore(false)]
[CheckDiscountCoupon]
[CheckAffiliate]
public class NopStationPublicController : BaseController
{
	protected virtual IActionResult InvokeHttp404()
	{
		base.Response.StatusCode = 404;
		return new EmptyResult();
	}
}
