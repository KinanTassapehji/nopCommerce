using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.Syriatel.Interfaces;
using Nop.Plugin.Payments.Syriatel.Services;

namespace Nop.Plugin.Payments.Syriatel.Infrastructure;
public class NopStartup : INopStartup
{
    public int Order => 1;

    public void Configure(IApplicationBuilder application)
    {
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddScoped<ISyriatelApiClient, SyriatelApiClient>();
        services.AddScoped<ISyriatelService, SyriatelService>();
        services.AddScoped<ISyriatelTokenProvider, SyriatelTokenProvider>();
        // services.AddMvc()
        //.AddRazorRuntimeCompilation()
        //.AddApplicationPart(typeof(ConfirmPaymentController).Assembly);
    }
}
