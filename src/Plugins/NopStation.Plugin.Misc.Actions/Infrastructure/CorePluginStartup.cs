using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure.Extensions;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.Core.Services.SMS;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class CorePluginStartup : INopStartup
{
	public int Order => 1;

	public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddMvc(delegate(MvcOptions configure)
		{
			configure.Filters.Add<CoreActionFilter>();
		});
		services.AddHttpClient<NopStationHttpClient>().WithProxy();
		services.AddScoped<ILicenseService, LicenseService>();
		services.AddScoped<ISmsPluginManager, SmsPluginManager>();
		services.AddScoped<INopStationPluginManager, NopStationPluginManager>();
		services.AddScoped<IProductAttributeParserApi, ProductAttributeParserApi>();
		services.AddScoped<ISmsService, SmsService>();
		services.AddScoped<IQueuedSmsService, QueuedSmsService>();
		services.AddScoped<ISmsTemplateService, SmsTemplateService>();
		services.AddScoped<ISmsTokenProvider, SmsTokenProvider>();
		services.AddScoped<IWorkflowSmsService, WorkflowSmsService>();
		services.AddScoped(typeof(ISettingHelper<, >), typeof(SettingHelper<, >));
	}

	public void Configure(IApplicationBuilder application)
	{
	}
}
