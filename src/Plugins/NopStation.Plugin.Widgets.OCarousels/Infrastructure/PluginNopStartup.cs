using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.OCarousels.Factories;
using NopStation.Plugin.Widgets.OCarousels.Services;

namespace NopStation.Plugin.Widgets.OCarousels.Infrastructure;

public class PluginNopStartup : INopStartup
{
	public int Order => 11;

	public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddNopStationServices("NopStation.Plugin.Widgets.OCarousels");
		services.AddScoped<IOCarouselService, OCarouselService>();
		services.AddScoped<NopStation.Plugin.Widgets.OCarousels.Factories.IOCarouselModelFactory, NopStation.Plugin.Widgets.OCarousels.Factories.OCarouselModelFactory>();
		services.AddScoped<NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Factories.IOCarouselModelFactory, NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Factories.OCarouselModelFactory>();
	}

	public void Configure(IApplicationBuilder application)
	{
	}
}
