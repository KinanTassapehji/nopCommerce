using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class EventConsumer : IConsumer<PageRenderingEvent>, IConsumer<AdminMenuEvent>
{
	private readonly IActionContextAccessor _actionContextAccessor;

	private readonly ILocalizationService _localizationService;

	private readonly IPermissionService _permissionService;

	public EventConsumer(IActionContextAccessor actionContextAccessor, ILocalizationService localizationService, IPermissionService permissionService)
	{
		_actionContextAccessor = actionContextAccessor;
		_localizationService = localizationService;
		_permissionService = permissionService;
	}

	public Task HandleEventAsync(PageRenderingEvent eventMessage)
	{
		object routeValue = _actionContextAccessor.ActionContext.HttpContext.GetRouteValue("area");
		if (routeValue != null && routeValue.ToString().Equals("admin", StringComparison.InvariantCultureIgnoreCase))
		{
			eventMessage.Helper.AppendCssFileParts("~/Plugins/NopStation.Core/contents/css/style.css");
		}
		return Task.CompletedTask;
	}

	public async Task HandleEventAsync(AdminMenuEvent createdEvent)
	{
		if (await _permissionService.AuthorizeAsync("ManageNopStationCoreConfiguration"))
		{
			NopStationAdminMenuItem nopStationAdminMenuItem = new NopStationAdminMenuItem();
			NopStationAdminMenuItem nopStationAdminMenuItem2 = nopStationAdminMenuItem;
			nopStationAdminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.Configuration");
			nopStationAdminMenuItem.Visible = true;
			nopStationAdminMenuItem.IconClass = "far fa-circle";
			nopStationAdminMenuItem.Url = "~/Admin/NopStationCore/Configure";
			nopStationAdminMenuItem.SystemName = "NopStationCore.Configure";
			createdEvent.CoreChildNodes.Add(nopStationAdminMenuItem);
			nopStationAdminMenuItem2 = new NopStationAdminMenuItem();
			nopStationAdminMenuItem = nopStationAdminMenuItem2;
			nopStationAdminMenuItem.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.LocaleResources");
			nopStationAdminMenuItem2.Visible = true;
			nopStationAdminMenuItem2.IconClass = "far fa-circle";
			nopStationAdminMenuItem2.Url = "~/Admin/NopStationCore/LocaleResource";
			nopStationAdminMenuItem2.SystemName = "NopStationCore.LocaleResources";
			createdEvent.CoreChildNodes.Add(nopStationAdminMenuItem2);
		}
		if (await _permissionService.AuthorizeAsync("Configuration.ManageACL"))
		{
			NopStationAdminMenuItem nopStationAdminMenuItem2 = new NopStationAdminMenuItem();
			NopStationAdminMenuItem nopStationAdminMenuItem = nopStationAdminMenuItem2;
			nopStationAdminMenuItem.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.ACL");
			nopStationAdminMenuItem2.Visible = true;
			nopStationAdminMenuItem2.IconClass = "far fa-circle";
			nopStationAdminMenuItem2.Url = "~/Admin/NopStationCore/Permissions";
			nopStationAdminMenuItem2.SystemName = "NopStationCore.ACL";
			createdEvent.CoreChildNodes.Add(nopStationAdminMenuItem2);
		}
		if (await _permissionService.AuthorizeAsync("ManageNopStationCoreLicense"))
		{
			NopStationAdminMenuItem nopStationAdminMenuItem = new NopStationAdminMenuItem();
			NopStationAdminMenuItem nopStationAdminMenuItem2 = nopStationAdminMenuItem;
			nopStationAdminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.License");
			nopStationAdminMenuItem.Visible = true;
			nopStationAdminMenuItem.IconClass = "far fa-circle";
			nopStationAdminMenuItem.Url = "~/Admin/NopStationLicense/License";
			nopStationAdminMenuItem.SystemName = "NopStationCore.License";
			createdEvent.CoreChildNodes.Add(nopStationAdminMenuItem);
		}
	}
}
