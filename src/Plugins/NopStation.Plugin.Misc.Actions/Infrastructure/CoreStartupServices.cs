using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public static class CoreStartupServices
{
	public static void AddNopStationServices(this IServiceCollection services, string folderName, bool rootAdmin = false, bool excludepublicView = false)
	{
		services.Configure(delegate(RazorViewEngineOptions options)
		{
			options.ViewLocationExpanders.Add(new ViewLocationExpander(folderName, rootAdmin, excludepublicView));
		});
	}
}
