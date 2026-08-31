using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Widgets.PrevNextProduct;

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
		if (await _permissionService.AuthorizeAsync("ManageNopStationPrevNextProduct"))
		{
			NopStationAdminMenuItem nopStationAdminMenuItem = new NopStationAdminMenuItem();
			NopStationAdminMenuItem nopStationAdminMenuItem2 = nopStationAdminMenuItem;
			nopStationAdminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.PrevNextProduct.Menu.PrevNextProduct");
			nopStationAdminMenuItem.Visible = true;
			nopStationAdminMenuItem.IconClass = "far fa-dot-circle";
			NopStationAdminMenuItem menuItem = nopStationAdminMenuItem;
			AdminMenuItem adminMenuItem = new AdminMenuItem();
			AdminMenuItem adminMenuItem2 = adminMenuItem;
			adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.PrevNextProduct.Menu.Configuration");
			adminMenuItem.Url = "~/Admin/PrevNextProduct/Configure";
			adminMenuItem.Visible = true;
			adminMenuItem.IconClass = "far fa-circle";
			adminMenuItem.SystemName = "PrevNextProduct.Configuration";
			menuItem.ChildNodes.Add(adminMenuItem);
			if (await _permissionService.AuthorizeAsync("ManageNopStationCoreConfiguration"))
			{
				adminMenuItem2 = new AdminMenuItem();
				adminMenuItem = adminMenuItem2;
				adminMenuItem.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation");
				adminMenuItem2.Url = "https://www.nop-station.com/previous-next-product-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=previous-next-product";
				adminMenuItem2.Visible = true;
				adminMenuItem2.IconClass = "far fa-circle";
				adminMenuItem2.OpenUrlInNewTab = true;
				menuItem.ChildNodes.Add(adminMenuItem2);
			}
			createdEvent.PluginChildNodes.Add(menuItem);
		}
	}
}
