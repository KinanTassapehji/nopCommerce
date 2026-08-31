using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Misc.CustomerReminders.Infrastructure;

public class CustomerRemindersRouteProvider : BaseRouteProvider, IRouteProvider
{
	public int Priority => 0;

	public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
	{
		endpointRouteBuilder.MapControllerRoute(CustomerRemindersDefaults.Route.Configuration, "Admin/CustomerReminders/Configure", new
		{
			controller = "CustomerReminders",
			action = "Configure",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(CustomerRemindersDefaults.Route.ReminderRules, "Admin/ReminderRule/List", new
		{
			controller = "ReminderRule",
			action = "List",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(CustomerRemindersDefaults.Route.Reminders, "Admin/Reminder/List", new
		{
			controller = "Reminder",
			action = "List",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(CustomerRemindersDefaults.Route.ReminderReports, "Admin/ReminderReport/List", new
		{
			controller = "ReminderReport",
			action = "List",
			area = "Admin"
		});
	}
}
