using System.Collections.Generic;

namespace NopStation.Plugin.Misc.Core.Models.Api;

public class BaseResponseModel
{
	public string Message { get; set; }

	public List<string> ErrorList { get; set; }

	public BaseResponseModel()
	{
		ErrorList = new List<string>();
	}
}
