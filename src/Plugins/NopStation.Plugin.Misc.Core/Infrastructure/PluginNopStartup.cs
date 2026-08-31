using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Areas.Admin.Factories;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class PluginNopStartup : INopStartup
{
	public int Order => 11;

	public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddNopStationServices(NopStationCoreDefaults.SystemName, rootAdmin: false, excludepublicView: true);
		services.AddScoped<INopStationContext, NopStationContext>();
		services.AddScoped<INopStationCustomerService, NopStationCustomerService>();
		services.AddHostedService<StartupEventHostedService>();
		services.AddScoped<ISmsModelFactory, SmsModelFactory>();
		services.AddScoped<IMarketplaceModelFactory, MarketplaceModelFactory>();
		services.AddScoped<ISmsTemplateModelFactory, SmsTemplateModelFactory>();
		services.AddScoped<IQueuedSmsModelFactory, QueuedSmsModelFactory>();
		services.AddHttpClient();
		services.AddHttpClient<IMarketplaceService, MarketplaceService>();
	}

	public void Configure(IApplicationBuilder application)
	{
	}
}
