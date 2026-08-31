using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;
using NopStation.Plugin.Misc.CustomerReminders.Services;
using NopStation.Plugin.Misc.CustomerReminders.Services.ReminderRules;

namespace NopStation.Plugin.Misc.CustomerReminders.Infrastructure;

public class PluginNopStartup : INopStartup
{
	public int Order => 100;

	public static void RegisterModelFactories(IServiceCollection services, IConfiguration configuration)
	{
		services.AddScoped<IConfigurationModelFactory, ConfigurationModelFactory>();
		services.AddScoped<IReminderModelFactory, ReminderModelFactory>();
		services.AddScoped<IReminderRuleModelFactory, ReminderRuleModelFactory>();
		services.AddScoped<IReminderReportModelFactory, ReminderReportModelFactory>();
	}

	public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddScoped<IReminderService, ReminderService>();
		services.AddScoped<IReminderRuleService, ReminderRuleService>();
		services.AddScoped<IReminderReportService, ReminderReportService>();
		services.AddScoped<IReminderReportExportService, ReminderReportExportService>();
		services.AddScoped<IReminderProcessingService, ReminderProcessingService>();
		services.AddScoped<IReminderExcludedCustomerService, ReminderExcludedCustomerService>();
		services.AddScoped<IReminderRuleImplementation, InactiveCustomersRule>();
		services.AddScoped<IReminderRuleImplementation, AbandonedCartRule>();
		services.AddScoped<IReminderRuleImplementation, UnpaidOrdersRule>();
		services.AddScoped<IReminderRuleImplementation, CompletedOrderRule>();
		services.AddScoped<IReminderRuleImplementation, BirthdayRule>();
		services.AddScoped<IReminderRuleImplementation, GenericReminderRule>();
		services.AddNopStationServices(CustomerRemindersDefaults.PluginSystemName);
	}

	public void Configure(IApplicationBuilder application)
	{
	}

	public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		RegisterModelFactories(services, configuration);
		RegisterServices(services, configuration);
	}
}
