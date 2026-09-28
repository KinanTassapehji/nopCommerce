using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Widgets.ProductTabs;

public class AdminMenuCreatedEventConsumer : IConsumer<AdminMenuEvent>, IConsumer<AdminMenuCreatedEvent>
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
		nopStationAdminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.Menu.ProductTab");
		nopStationAdminMenuItem.Visible = true;
		nopStationAdminMenuItem.IconClass = "far fa-dot-circle";
		NopStationAdminMenuItem menuItem = nopStationAdminMenuItem;
		if (await _permissionService.AuthorizeAsync("ManageNopStationProductTab"))
		{
			//the list lives under Content management, see the AdminMenuCreatedEvent handler below
			AdminMenuItem adminMenuItem = new AdminMenuItem();
			AdminMenuItem adminMenuItem2 = adminMenuItem;
			adminMenuItem.Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.Menu.Configuration");
			adminMenuItem2.Url = "~/Admin/ProductTab/Configure";
			adminMenuItem2.Visible = true;
			adminMenuItem2.IconClass = "far fa-dot-circle";
			adminMenuItem2.SystemName = "ProductTabs.Configuration";
			menuItem.ChildNodes.Add(adminMenuItem2);
		}
		if (menuItem.ChildNodes.Any())
		{
			AdminMenuItem adminMenuItem = new AdminMenuItem();
			AdminMenuItem adminMenuItem2 = adminMenuItem;
			adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation");
			adminMenuItem.Url = "https://www.nop-station.com/product-tab-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=product-tab";
			adminMenuItem.Visible = true;
			adminMenuItem.IconClass = "far fa-circle";
			adminMenuItem.OpenUrlInNewTab = true;
			menuItem.ChildNodes.Add(adminMenuItem);
			createdEvent.PluginChildNodes.Add(menuItem);
		}
	}

	public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
	{
		AdminMenuItem listItem = new AdminMenuItem
		{
			SystemName = "ProductTabs",
			Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductTabs.Menu.List"),
			Url = "~/Admin/ProductTab/List",
			IconClass = "far fa-dot-circle",
			PermissionNames = new List<string>(1) { "ManageNopStationProductTab" }
		};
		var contentNode = eventMessage.RootMenuItem.ChildNodes.FirstOrDefault(node => node.SystemName == "Content Management");
		if (contentNode != null && !contentNode.ContainsSystemName(listItem.SystemName) && !contentNode.InsertAfter("Home page slider", listItem))
			contentNode.ChildNodes.Add(listItem);
	}
}
