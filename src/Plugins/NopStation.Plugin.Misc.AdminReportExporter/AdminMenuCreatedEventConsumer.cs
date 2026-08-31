using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.AdminReportExporter;

public class AdminMenuCreatedEventConsumer : IConsumer<AdminMenuEvent>
{
	private readonly ILocalizationService _localizationService;

	private readonly IPermissionService _permissionService;

	public AdminMenuCreatedEventConsumer(ILocalizationService localizationService, IPermissionService permissionService)
	{
		_localizationService = localizationService;
		_permissionService = permissionService;
	}

	public async Task HandleEventAsync(AdminMenuEvent createdEvent)
	{
		NopStationAdminMenuItem nopStationAdminMenuItem = new NopStationAdminMenuItem
		{
			Visible = true,
			IconClass = "far fa-dot-circle"
		};
		NopStationAdminMenuItem nopStationAdminMenuItem2 = nopStationAdminMenuItem;
		nopStationAdminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.AdminReportExporter.Menu.AdminReportExporter");
		NopStationAdminMenuItem menu = nopStationAdminMenuItem;
		if (await _permissionService.AuthorizeAsync("ManageAdminReportExporter"))
		{
			AdminMenuItem adminMenuItem = new AdminMenuItem
			{
				Visible = true,
				IconClass = "far fa-circle",
				Url = "~/Admin/AdminReportExporter/Configure"
			};
			AdminMenuItem adminMenuItem2 = adminMenuItem;
			adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.AdminReportExporter.Menu.Configuration");
			adminMenuItem.SystemName = "AdminReportExporter.Configuration";
			menu.ChildNodes.Add(adminMenuItem);
		}
		if (menu.ChildNodes.Any())
		{
			if (await _permissionService.AuthorizeAsync("ShowNopStationDocumentations"))
			{
				AdminMenuItem adminMenuItem2 = new AdminMenuItem();
				AdminMenuItem adminMenuItem = adminMenuItem2;
				adminMenuItem.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation");
				adminMenuItem2.Url = "https://www.nop-station.com/admin-report-exporter-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=admin-report-exporter";
				adminMenuItem2.Visible = true;
				adminMenuItem2.IconClass = "far fa-circle";
				adminMenuItem2.OpenUrlInNewTab = true;
				menu.ChildNodes.Add(adminMenuItem2);
			}
			createdEvent.PluginChildNodes.Add(menu);
		}
	}
}
