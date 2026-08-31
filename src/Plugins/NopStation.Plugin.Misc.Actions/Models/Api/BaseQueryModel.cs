using System;
using System.Collections.Generic;

namespace NopStation.Plugin.Misc.Core.Models.Api;

public class BaseQueryModel<TModel>
{
	public TModel Data { get; set; }

	public List<KeyValueApi> FormValues { get; set; }

	public PictureQueryModel UploadPicture { get; set; }

	public BaseQueryModel()
	{
		if (typeof(TModel).GetConstructor(Type.EmptyTypes) != null)
		{
			Data = Activator.CreateInstance<TModel>();
		}
		FormValues = new List<KeyValueApi>();
		UploadPicture = new PictureQueryModel();
	}
}
