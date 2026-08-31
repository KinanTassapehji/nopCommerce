using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core.Services.SMS;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class SmsAdminMenuEventConsumer : IConsumer<AdminMenuEvent>
{
	private readonly ILocalizationService _localizationService;

	private readonly IPermissionService _permissionService;

	private readonly ISmsPluginManager _smsPluginManager;

	public SmsAdminMenuEventConsumer(ILocalizationService localizationService, IPermissionService permissionService, ISmsPluginManager smsPluginManager)
	{
		_localizationService = localizationService;
		_permissionService = permissionService;
		_smsPluginManager = smsPluginManager;
	}

	public async Task HandleEventAsync(AdminMenuEvent createdEvent)
	{
		if ((await _smsPluginManager.LoadSmsPluginsAsync()).Any())
		{
			NopStationAdminMenuItem nopStationAdminMenuItem = new NopStationAdminMenuItem
			{
				Visible = true,
				IconClass = "fas fa-sms"
			};
			NopStationAdminMenuItem nopStationAdminMenuItem2 = nopStationAdminMenuItem;
			nopStationAdminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.Sms");
			NopStationAdminMenuItem menu = nopStationAdminMenuItem;
			if (await _permissionService.AuthorizeAsync("ManageNopStationSmsConfiguration"))
			{
				IList<AdminMenuItem> childNodes = menu.ChildNodes;
				AdminMenuItem adminMenuItem = new AdminMenuItem
				{
					Visible = true,
					IconClass = "far fa-dot-circle",
					Url = "~/Admin/Sms/Configure"
				};
				AdminMenuItem adminMenuItem2 = adminMenuItem;
				adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.SmsSettings");
				adminMenuItem.SystemName = "SmsActions.SmsSettings";
				childNodes.Add(adminMenuItem);
			}
			if (await _permissionService.AuthorizeAsync("ManageNopStationSmsProviders"))
			{
				IList<AdminMenuItem> childNodes = menu.ChildNodes;
				AdminMenuItem adminMenuItem2 = new AdminMenuItem
				{
					Visible = true,
					IconClass = "far fa-dot-circle",
					Url = "~/Admin/Sms/Providers"
				};
				AdminMenuItem adminMenuItem = adminMenuItem2;
				adminMenuItem.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.SmsProviders");
				adminMenuItem2.SystemName = "SmsActions.SmsProviders";
				childNodes.Add(adminMenuItem2);
			}
			if (await _permissionService.AuthorizeAsync("ManageNopStationSmsTemplates"))
			{
				IList<AdminMenuItem> childNodes = menu.ChildNodes;
				AdminMenuItem adminMenuItem = new AdminMenuItem
				{
					Visible = true,
					IconClass = "far fa-dot-circle",
					Url = "~/Admin/SmsTemplate/List"
				};
				AdminMenuItem adminMenuItem2 = adminMenuItem;
				adminMenuItem2.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.SmsTemplates");
				adminMenuItem.SystemName = "SmsActions.SmsTemplates";
				childNodes.Add(adminMenuItem);
			}
			if (await _permissionService.AuthorizeAsync("ManageNopStationSmsQueue"))
			{
				IList<AdminMenuItem> childNodes = menu.ChildNodes;
				AdminMenuItem adminMenuItem2 = new AdminMenuItem
				{
					Visible = true,
					IconClass = "far fa-dot-circle",
					Url = "~/Admin/QueuedSms/List"
				};
				AdminMenuItem adminMenuItem = adminMenuItem2;
				adminMenuItem.Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.QueuedSms");
				adminMenuItem2.SystemName = "SmsActions.QueuedSms";
				childNodes.Add(adminMenuItem2);
			}
			if (menu.ChildNodes.Any())
			{
				createdEvent.RootChildNodes.Add(menu);
			}
		}
	}
}
