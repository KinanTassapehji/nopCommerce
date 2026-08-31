using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.AnywhereSlider.Factories;
using NopStation.Plugin.Widgets.AnywhereSlider.Services;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Infrastructure;

public class PluginNopStartup : INopStartup
{
	public int Order => 11;

	public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddNopStationServices("NopStation.Plugin.Widgets.AnywhereSlider");
		services.AddScoped<ISliderService, SliderService>();
		services.AddScoped<NopStation.Plugin.Widgets.AnywhereSlider.Factories.ISliderModelFactory, NopStation.Plugin.Widgets.AnywhereSlider.Factories.SliderModelFactory>();
		services.AddScoped<NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Factories.ISliderModelFactory, NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Factories.SliderModelFactory>();
	}

	public void Configure(IApplicationBuilder application)
	{
	}
}
