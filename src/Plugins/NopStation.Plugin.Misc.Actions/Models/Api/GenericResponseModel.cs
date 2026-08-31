using System;

namespace NopStation.Plugin.Misc.Core.Models.Api;

public class GenericResponseModel<TResult> : BaseResponseModel
{
	public TResult Data { get; set; }

	public GenericResponseModel()
	{
		if (typeof(TResult).GetConstructor(Type.EmptyTypes) != null)
		{
			Data = Activator.CreateInstance<TResult>();
		}
	}
}
