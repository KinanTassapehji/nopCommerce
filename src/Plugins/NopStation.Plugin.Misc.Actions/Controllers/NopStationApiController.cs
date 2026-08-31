using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Services.Localization;
using Nop.Web.Framework.Models;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.Core.Controllers;

[NopStationApiLicense]
[ApiController]
public class NopStationApiController : Controller
{
	protected IActionResult OkWrap<T>(T baseModel, string message = null, IList<string> errors = null, bool defaultMessage = false) where T : BaseNopModel
	{
		GenericResponseModel<T> genericResponseModel = new GenericResponseModel<T>();
		genericResponseModel.Data = baseModel;
		if (defaultMessage && string.IsNullOrWhiteSpace(message))
		{
			message = NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.Ok").Result;
		}
		genericResponseModel.Message = message;
		if (errors != null && errors.Any())
		{
			genericResponseModel.ErrorList.AddRange(errors);
		}
		return base.Ok(genericResponseModel);
	}

	protected IActionResult Ok(string message = null, IList<string> errors = null, bool defaultMessage = false)
	{
		BaseResponseModel baseResponseModel = new BaseResponseModel();
		if (defaultMessage && string.IsNullOrWhiteSpace(message))
		{
			message = NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.Ok").Result;
		}
		baseResponseModel.Message = message;
		if (errors != null && errors.Any())
		{
			baseResponseModel.ErrorList.AddRange(errors);
		}
		return base.Ok(baseResponseModel);
	}

	protected IActionResult Created(int id, string message = null, IList<string> errors = null, bool defaultMessage = true)
	{
		GenericResponseModel<int> genericResponseModel = new GenericResponseModel<int>();
		genericResponseModel.Data = id;
		if (defaultMessage && string.IsNullOrWhiteSpace(message))
		{
			message = NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.Ok").Result;
		}
		genericResponseModel.Message = message;
		if (errors != null && errors.Any())
		{
			genericResponseModel.ErrorList.AddRange(errors);
		}
		return base.StatusCode(201, genericResponseModel);
	}

	protected IActionResult BadRequestWrap<T>(T baseModel, ModelStateDictionary modelState = null, IList<string> errors = null, bool showDefaultMessageIfEmpty = true) where T : BaseNopModel
	{
		GenericResponseModel<T> genericResponseModel = new GenericResponseModel<T>();
		genericResponseModel.Data = baseModel;
		if (modelState != null && !modelState.IsValid)
		{
			genericResponseModel.ErrorList.AddRange(modelState.GetErrors());
		}
		if (errors != null && errors.Any())
		{
			genericResponseModel.ErrorList.AddRange(errors);
		}
		if (!genericResponseModel.ErrorList.Any() & showDefaultMessageIfEmpty)
		{
			genericResponseModel.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.BadRequest").Result);
		}
		return base.BadRequest(genericResponseModel);
	}

	protected IActionResult BadRequest(string error = null, IList<string> errors = null, bool defaultMessage = true)
	{
		BaseResponseModel baseResponseModel = new BaseResponseModel();
		if (!string.IsNullOrWhiteSpace(error))
		{
			baseResponseModel.ErrorList.Add(error);
		}
		if (errors != null && errors.Any())
		{
			baseResponseModel.ErrorList.AddRange(errors);
		}
		if (!baseResponseModel.ErrorList.Any() & defaultMessage)
		{
			baseResponseModel.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.BadRequest").Result);
		}
		return base.BadRequest(baseResponseModel);
	}

	protected IActionResult UnauthorizedWrap<T>(T baseModel, string error = null, IList<string> errors = null, bool defaultMessage = true) where T : BaseNopModel
	{
		GenericResponseModel<T> genericResponseModel = new GenericResponseModel<T>();
		genericResponseModel.Data = baseModel;
		if (!string.IsNullOrWhiteSpace(error))
		{
			genericResponseModel.ErrorList.Add(error);
		}
		if (errors != null && errors.Any())
		{
			genericResponseModel.ErrorList.AddRange(errors);
		}
		if (!genericResponseModel.ErrorList.Any() & defaultMessage)
		{
			genericResponseModel.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.Unauthorized").Result);
		}
		return base.Unauthorized(genericResponseModel);
	}

	protected IActionResult Unauthorized(string error = null, IList<string> errors = null, bool defaultMessage = true)
	{
		BaseResponseModel baseResponseModel = new BaseResponseModel();
		if (!string.IsNullOrWhiteSpace(error))
		{
			baseResponseModel.ErrorList.Add(error);
		}
		if (errors != null && errors.Any())
		{
			baseResponseModel.ErrorList.AddRange(errors);
		}
		if (!baseResponseModel.ErrorList.Any() & defaultMessage)
		{
			baseResponseModel.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.Unauthorized").Result);
		}
		return base.Unauthorized(baseResponseModel);
	}

	protected IActionResult NotFoundWrap<T>(T baseModel, string error = null, IList<string> errors = null, bool defaultMessage = true) where T : BaseNopModel
	{
		GenericResponseModel<T> genericResponseModel = new GenericResponseModel<T>();
		genericResponseModel.Data = baseModel;
		if (!string.IsNullOrWhiteSpace(error))
		{
			genericResponseModel.ErrorList.Add(error);
		}
		if (errors != null && errors.Any())
		{
			genericResponseModel.ErrorList.AddRange(errors);
		}
		if (!genericResponseModel.ErrorList.Any() & defaultMessage)
		{
			genericResponseModel.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.NotFound").Result);
		}
		return base.NotFound(genericResponseModel);
	}

	protected IActionResult NotFound(string error = null, IList<string> errors = null, bool defaultMessage = true)
	{
		BaseResponseModel baseResponseModel = new BaseResponseModel();
		if (!string.IsNullOrWhiteSpace(error))
		{
			baseResponseModel.ErrorList.Add(error);
		}
		if (errors != null && errors.Any())
		{
			baseResponseModel.ErrorList.AddRange(errors);
		}
		if (!baseResponseModel.ErrorList.Any() & defaultMessage)
		{
			baseResponseModel.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.NotFound").Result);
		}
		return base.NotFound(baseResponseModel);
	}

	protected IActionResult InternalServerErrorWrap<T>(T baseModel, string error = null, IList<string> errors = null, bool defaultMessage = true) where T : BaseNopModel
	{
		GenericResponseModel<T> genericResponseModel = new GenericResponseModel<T>();
		genericResponseModel.Data = baseModel;
		if (!string.IsNullOrWhiteSpace(error))
		{
			genericResponseModel.ErrorList.Add(error);
		}
		if (errors != null && errors.Any())
		{
			genericResponseModel.ErrorList.AddRange(errors);
		}
		if (!genericResponseModel.ErrorList.Any() & defaultMessage)
		{
			genericResponseModel.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.InternalServerError").Result);
		}
		return base.StatusCode(500, genericResponseModel);
	}

	protected IActionResult InternalServerError(string error = null, IList<string> errors = null, bool defaultMessage = true)
	{
		BaseResponseModel baseResponseModel = new BaseResponseModel();
		if (!string.IsNullOrWhiteSpace(error))
		{
			baseResponseModel.ErrorList.Add(error);
		}
		if (errors != null && errors.Any())
		{
			baseResponseModel.ErrorList.AddRange(errors);
		}
		if (!baseResponseModel.ErrorList.Any() & defaultMessage)
		{
			baseResponseModel.ErrorList.Add(NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.Core.Request.Common.InternalServerError").Result);
		}
		return base.StatusCode(500, baseResponseModel);
	}

	protected IActionResult MethodNotAllowed()
	{
		return base.StatusCode(405);
	}

	protected IActionResult LengthRequired()
	{
		return base.StatusCode(411);
	}
}
