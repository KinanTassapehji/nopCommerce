using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Widgets.FirebasePushNotification.Extensions;
using Widgets.FirebasePushNotification.Services;

namespace Widgets.FirebasePushNotification.Infrastructure;

public class DependencyRegistrar : INopStartup
{
	public int Order => 3000;

	public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddHttpClient();
		services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();
		services.AddFirebase();
	}

	public void Configure(IApplicationBuilder application)
	{
	}
}
