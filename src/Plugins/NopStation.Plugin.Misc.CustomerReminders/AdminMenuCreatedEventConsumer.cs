using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Mvc.Routing;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.CustomerReminders;

public class AdminMenuCreatedEventConsumer : IConsumer<AdminMenuEvent>
{
	private readonly ILocalizationService _localizationService;

	private readonly IPermissionService _permissionService;

	private readonly INopUrlHelper _nopUrlHelper;

	public AdminMenuCreatedEventConsumer(ILocalizationService localizationService, IPermissionService permissionService, INopUrlHelper nopUrlHelper)
	{
		_localizationService = localizationService;
		_permissionService = permissionService;
		_nopUrlHelper = nopUrlHelper;
	}

	public async Task HandleEventAsync(AdminMenuEvent createdEvent)
	{
		if (!(await _permissionService.AuthorizeAsync("ManageCustomerReminders")))
		{
			return;
		}
		NopStationAdminMenuItem nopStationAdminMenuItem = new NopStationAdminMenuItem();
		NopStationAdminMenuItem nopStationAdminMenuItem2 = nopStationAdminMenuItem;
		nopStationAdminMenuItem2.Title = await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Menu.CustomerReminders");
		nopStationAdminMenuItem.Visible = true;
		nopStationAdminMenuItem.IconClass = "far fa-dot-circle";
		nopStationAdminMenuItem.SystemName = CustomerRemindersDefaults.PluginMenuSystemName;
		NopStationAdminMenuItem menuItem = nopStationAdminMenuItem;
		AdminMenuItem adminMenuItem = new AdminMenuItem();
		AdminMenuItem adminMenuItem2 = adminMenuItem;
		adminMenuItem2.Title = await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Menu.Configuration");
		adminMenuItem.Url = _nopUrlHelper.RouteUrl(CustomerRemindersDefaults.Route.Configuration);
		adminMenuItem.Visible = true;
		adminMenuItem.IconClass = "far fa-circle";
		adminMenuItem.SystemName = "NopStation.CustomerReminders.Configuration";
		menuItem.ChildNodes.Add(adminMenuItem);
		adminMenuItem2 = new AdminMenuItem();
		adminMenuItem = adminMenuItem2;
		adminMenuItem.Title = await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Menu.ReminderRules");
		adminMenuItem2.Url = _nopUrlHelper.RouteUrl(CustomerRemindersDefaults.Route.ReminderRules);
		adminMenuItem2.Visible = true;
		adminMenuItem2.IconClass = "far fa-circle";
		adminMenuItem2.SystemName = "NopStation.CustomerReminders.ReminderRules";
		menuItem.ChildNodes.Add(adminMenuItem2);
		adminMenuItem = new AdminMenuItem();
		adminMenuItem2 = adminMenuItem;
		adminMenuItem2.Title = await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Menu.Reminders");
		adminMenuItem.Url = _nopUrlHelper.RouteUrl(CustomerRemindersDefaults.Route.Reminders);
		adminMenuItem.Visible = true;
		adminMenuItem.IconClass = "far fa-circle";
		adminMenuItem.SystemName = "NopStation.CustomerReminders.Reminders";
		menuItem.ChildNodes.Add(adminMenuItem);
		adminMenuItem2 = new AdminMenuItem();
		adminMenuItem = adminMenuItem2;
		adminMenuItem.Title = await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Menu.ReminderReports");
		adminMenuItem2.Url = _nopUrlHelper.RouteUrl(CustomerRemindersDefaults.Route.ReminderReports);
		adminMenuItem2.Visible = true;
		adminMenuItem2.IconClass = "far fa-circle";
		adminMenuItem2.SystemName = "NopStation.CustomerReminders.ReminderReports";
		menuItem.ChildNodes.Add(adminMenuItem2);
		if (menuItem.ChildNodes.Any())
		{
			if (await _permissionService.AuthorizeAsync("ShowNopStationDocumentations"))
			{
				adminMenuItem = new AdminMenuItem();
				adminMenuItem2 = adminMenuItem;
				adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation");
				adminMenuItem.Url = "https://www.nop-station.com/customer-reminders-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=customer-reminders";
				adminMenuItem.Visible = true;
				adminMenuItem.IconClass = "far fa-circle";
				adminMenuItem.OpenUrlInNewTab = true;
				menuItem.ChildNodes.Add(adminMenuItem);
			}
			createdEvent.PluginChildNodes.Add(menuItem);
		}
	}
}
