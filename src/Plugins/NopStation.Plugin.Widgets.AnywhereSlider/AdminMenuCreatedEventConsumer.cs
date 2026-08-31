using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Widgets.AnywhereSlider;

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
		NopStationAdminMenuItem nopStationAdminMenuItem = new NopStationAdminMenuItem();
		NopStationAdminMenuItem nopStationAdminMenuItem2 = nopStationAdminMenuItem;
		nopStationAdminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Menu.AnywhereSlider");
		nopStationAdminMenuItem.Visible = true;
		nopStationAdminMenuItem.IconClass = "far fa-dot-circle";
		NopStationAdminMenuItem menuItem = nopStationAdminMenuItem;
		if (await _permissionService.AuthorizeAsync("ManageNopStationSliders"))
		{
			AdminMenuItem adminMenuItem = new AdminMenuItem();
			AdminMenuItem adminMenuItem2 = adminMenuItem;
			adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Menu.Sliders");
			adminMenuItem.Url = "~/Admin/AnywhereSlider/List";
			adminMenuItem.Visible = true;
			adminMenuItem.IconClass = "far fa-circle";
			adminMenuItem.SystemName = "AnywhereSlider";
			menuItem.ChildNodes.Add(adminMenuItem);
			adminMenuItem2 = new AdminMenuItem();
			adminMenuItem = adminMenuItem2;
			adminMenuItem.Title = await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Menu.Configuration");
			adminMenuItem2.Url = "~/Admin/AnywhereSlider/Configure";
			adminMenuItem2.Visible = true;
			adminMenuItem2.IconClass = "far fa-circle";
			adminMenuItem2.SystemName = "AnywhereSlider.Configuration";
			menuItem.ChildNodes.Add(adminMenuItem2);
		}
		if (menuItem.ChildNodes.Any())
		{
			AdminMenuItem adminMenuItem = new AdminMenuItem();
			AdminMenuItem adminMenuItem2 = adminMenuItem;
			adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation");
			adminMenuItem.Url = "https://www.nop-station.com/anywhere-slider-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=anywhere-slider";
			adminMenuItem.Visible = true;
			adminMenuItem.IconClass = "far fa-circle";
			adminMenuItem.OpenUrlInNewTab = true;
			menuItem.ChildNodes.Add(adminMenuItem);
			createdEvent.PluginChildNodes.Add(menuItem);
		}
	}
}
