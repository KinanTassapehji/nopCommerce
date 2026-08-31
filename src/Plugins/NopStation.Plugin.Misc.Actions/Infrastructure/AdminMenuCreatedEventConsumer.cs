using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class AdminMenuCreatedEventConsumer : IConsumer<AdminMenuCreatedEvent>
{
	private readonly IEventPublisher _eventPublisher;

	private readonly ILocalizationService _localizationService;

	public AdminMenuCreatedEventConsumer(IEventPublisher eventPublisher, ILocalizationService localizationService)
	{
		_eventPublisher = eventPublisher;
		_localizationService = localizationService;
	}

	public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
	{
		AdminMenuEvent createdEvent = new AdminMenuEvent();
		await _eventPublisher.PublishAsync(createdEvent);
		string documentationTitle = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation");
		RemoveDocumentationNodes(createdEvent.PluginChildNodes, documentationTitle);
		RemoveDocumentationNodes(createdEvent.ThemeChildNodes, documentationTitle);
		RemoveDocumentationNodes(createdEvent.CoreChildNodes, documentationTitle);
		RemoveDocumentationNodes(createdEvent.RootChildNodes, documentationTitle);
		CollapseSingleChildNodes(createdEvent.PluginChildNodes);
		CollapseSingleChildNodes(createdEvent.ThemeChildNodes);
		CollapseSingleChildNodes(createdEvent.CoreChildNodes);
		CollapseSingleChildNodes(createdEvent.RootChildNodes);
		AdminMenuItem adminMenuItem = new AdminMenuItem
		{
			SystemName = "NopStation"
		};
		AdminMenuItem adminMenuItem2 = adminMenuItem;
		adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.NopStation");
		adminMenuItem.IconClass = "icon icon-nop-station";
		adminMenuItem.Visible = false;
		AdminMenuItem nopStationMenu = adminMenuItem;
		IList<AdminMenuItem> childNodes;
		if (createdEvent.PluginChildNodes.Any())
		{
			childNodes = nopStationMenu.ChildNodes;
			adminMenuItem2 = new AdminMenuItem();
			adminMenuItem = adminMenuItem2;
			adminMenuItem.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.Plugins");
			adminMenuItem2.SystemName = "NopStationPlugin";
			adminMenuItem2.IconClass = "icon icon-plugins";
			adminMenuItem2.ChildNodes = OrderNodes(createdEvent.PluginChildNodes);
			childNodes.Add(adminMenuItem2);
		}
		if (createdEvent.ThemeChildNodes.Any())
		{
			childNodes = nopStationMenu.ChildNodes;
			adminMenuItem = new AdminMenuItem();
			adminMenuItem2 = adminMenuItem;
			adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.Themes");
			adminMenuItem.SystemName = "NopStationTheme";
			adminMenuItem.IconClass = "icon icon-themes";
			adminMenuItem.ChildNodes = OrderNodes(createdEvent.ThemeChildNodes);
			childNodes.Add(adminMenuItem);
		}
		if (createdEvent.CoreChildNodes.Any())
		{
			childNodes = nopStationMenu.ChildNodes;
			adminMenuItem2 = new AdminMenuItem();
			adminMenuItem = adminMenuItem2;
			adminMenuItem.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.Core");
			adminMenuItem2.SystemName = "NopStationCore";
			adminMenuItem2.IconClass = "fa fa-wrench";
			adminMenuItem2.ChildNodes = OrderNodes(createdEvent.CoreChildNodes);
			childNodes.Add(adminMenuItem2);
		}
		foreach (AdminMenuItem item in OrderNodes(createdEvent.RootChildNodes))
		{
			nopStationMenu.ChildNodes.Add(item);
		}
		// ponytail: "Report a bug" and "Assembly information" menu items removed
		eventMessage.RootMenuItem.ChildNodes.Add(nopStationMenu);
	}

	//ponytail: every NopStation plugin adds its own "Documentation" link, but they all funnel through
	//this consumer - pruning here once beats editing 13 plugin assemblies. Matched on the resolved
	//resource title, so it stays correct in any language.
	private static void RemoveDocumentationNodes(IList<NopStationAdminMenuItem> nodes, string documentationTitle)
	{
		if (string.IsNullOrEmpty(documentationTitle))
			return;

		foreach (NopStationAdminMenuItem node in nodes)
			RemoveDocumentationNodes(node.ChildNodes, documentationTitle);

		foreach (NopStationAdminMenuItem node in nodes.Where(x => x.Title == documentationTitle).ToList())
			nodes.Remove(node);
	}

	private static void RemoveDocumentationNodes(IList<AdminMenuItem> nodes, string documentationTitle)
	{
		foreach (AdminMenuItem node in nodes)
			RemoveDocumentationNodes(node.ChildNodes, documentationTitle);

		foreach (AdminMenuItem node in nodes.Where(x => x.Title == documentationTitle).ToList())
			nodes.Remove(node);
	}

	//ponytail: pruning the documentation links above leaves most NopStation plugins with a single
	//"Configuration" child, which the sidebar still draws as a dropdown you must open to reach one
	//item. Lift that lone child's link onto its parent so the parent is the link. Parents with two
	//or more children (OCarousels, CustomerReminders) are untouched.
	//IEnumerable is covariant, so this takes the NopStationAdminMenuItem lists as well.
	private static void CollapseSingleChildNodes(IEnumerable<AdminMenuItem> nodes)
	{
		foreach (AdminMenuItem node in nodes)
		{
			//bottom-up, so a nested group that collapses to one link collapses its parent too
			CollapseSingleChildNodes(node.ChildNodes);

			if (!string.IsNullOrEmpty(node.Url) || node.ChildNodes.Count != 1)
				continue;

			AdminMenuItem only = node.ChildNodes[0];
			if (string.IsNullOrEmpty(only.Url) || !only.Visible)
				continue;

			//keep the parent's title and icon - that is what the sidebar shows - and take the child's
			//target. The system name has to come across too or the active-item highlight stops matching,
			//because it was the child that carried the name the controllers set.
			node.Url = only.Url;
			node.OpenUrlInNewTab = only.OpenUrlInNewTab;
			node.SystemName = only.SystemName;
			if (only.PermissionNames.Any())
				node.PermissionNames = only.PermissionNames;
			node.ChildNodes.Clear();
		}
	}

	private IList<AdminMenuItem> OrderNodes(IList<NopStationAdminMenuItem> pluginChildNodes)
	{
		List<AdminMenuItem> list = new List<AdminMenuItem>();
		foreach (NopStationAdminMenuItem item in pluginChildNodes.OrderBy((NopStationAdminMenuItem x) => x.DisplayOrer))
		{
			list.Add(new AdminMenuItem
			{
				Title = item.Title,
				SystemName = EnsureSystemNameNotNull(item.SystemName),
				IconClass = item.IconClass,
				Url = item.Url,
				OpenUrlInNewTab = item.OpenUrlInNewTab,
				PermissionNames = item.PermissionNames,
				Visible = item.Visible,
				ChildNodes = EnsureSystemNameNotNull(item.ChildNodes)
			});
		}
		return list;
	}

	private IList<AdminMenuItem> EnsureSystemNameNotNull(IList<AdminMenuItem> nodes)
	{
		if (!nodes.Any())
		{
			return nodes;
		}
		foreach (AdminMenuItem node in nodes)
		{
			node.SystemName = EnsureSystemNameNotNull(node.SystemName);
			if (node.ChildNodes.Any())
			{
				node.ChildNodes = EnsureSystemNameNotNull(node.ChildNodes);
			}
		}
		return nodes;
	}

	private string EnsureSystemNameNotNull(string systemName)
	{
		if (!string.IsNullOrEmpty(systemName))
		{
			return systemName;
		}
		return Guid.NewGuid().ToString();
	}
}
