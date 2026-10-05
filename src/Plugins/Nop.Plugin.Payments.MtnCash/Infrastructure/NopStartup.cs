using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.MtnCash.Interfaces;
using Nop.Plugin.Payments.MtnCash.Services;

namespace Nop.Plugin.Payments.MtnCash.Infrastructure;
public class NopStartup : INopStartup
{
    public int Order => 1;

    public void Configure(IApplicationBuilder application)
    {
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddScoped<IMtnApiClient, MtnApiClient>();
        services.AddScoped<IMtnCashService, MtnCashService>();
        // services.AddMvc()
        //.AddRazorRuntimeCompilation()
        //.AddApplicationPart(typeof(ConfirmPaymentController).Assembly);
    }
}
