using System;

namespace NopStation.Plugin.Misc.Core.Helpers;

public static class UriHelper
{
	private static Uri Concat(this Uri uri, string path)
	{
		return new Uri(new Uri(uri.AbsoluteUri.TrimEnd('/') + "/"), path.TrimStart('/'));
	}

	public static Uri Concat(this Uri uri, params string[] paths)
	{
		foreach (string path in paths)
		{
			uri = uri.Concat(path);
		}
		return uri;
	}
}
