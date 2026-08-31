using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Widgets.MegaMenu;

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
		if (await _permissionService.AuthorizeAsync("ManageNopStationMegaMenu"))
		{
			NopStationAdminMenuItem nopStationAdminMenuItem = new NopStationAdminMenuItem();
			NopStationAdminMenuItem nopStationAdminMenuItem2 = nopStationAdminMenuItem;
			nopStationAdminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.Menu.MegaMenu");
			nopStationAdminMenuItem.Visible = true;
			nopStationAdminMenuItem.IconClass = "far fa-dot-circle";
			NopStationAdminMenuItem menuItem = nopStationAdminMenuItem;
			AdminMenuItem adminMenuItem = new AdminMenuItem
			{
				Visible = true,
				IconClass = "far fa-circle",
				Url = "/Admin/CategoryIcon/List"
			};
			AdminMenuItem adminMenuItem2 = adminMenuItem;
			adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.CategoryIcons");
			adminMenuItem.SystemName = "MegaMenuCategoryIcon";
			menuItem.ChildNodes.Add(adminMenuItem);
			adminMenuItem2 = new AdminMenuItem();
			adminMenuItem = adminMenuItem2;
			adminMenuItem.Title = await _localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.Menu.Configuration");
			adminMenuItem2.Url = "/Admin/MegaMenu/Configure";
			adminMenuItem2.Visible = true;
			adminMenuItem2.IconClass = "far fa-circle";
			adminMenuItem2.SystemName = "MegaMenu.Configuration";
			menuItem.ChildNodes.Add(adminMenuItem2);
			adminMenuItem = new AdminMenuItem();
			adminMenuItem2 = adminMenuItem;
			adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation");
			adminMenuItem.Url = "https://www.nop-station.com/mega-menu-plugin";
			adminMenuItem.Visible = true;
			adminMenuItem.IconClass = "far fa-circle";
			adminMenuItem.OpenUrlInNewTab = true;
			menuItem.ChildNodes.Add(adminMenuItem);
			createdEvent.PluginChildNodes.Add(menuItem);
		}
	}
}
