using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProductTabs.Factories;
using NopStation.Plugin.Widgets.ProductTabs.Services;

namespace NopStation.Plugin.Widgets.ProductTabs.Infrastructure;

public class PluginNopStartup : INopStartup
{
	public int Order => 11;

	public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddNopStationServices("NopStation.Plugin.Widgets.ProductTabs");
		services.AddScoped<IProductTabService, ProductTabService>();
		services.AddScoped<NopStation.Plugin.Widgets.ProductTabs.Factories.IProductTabModelFactory, NopStation.Plugin.Widgets.ProductTabs.Factories.ProductTabModelFactory>();
		services.AddScoped<NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Factories.IProductTabModelFactory, NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Factories.ProductTabModelFactory>();
	}

	public void Configure(IApplicationBuilder application)
	{
	}
}
