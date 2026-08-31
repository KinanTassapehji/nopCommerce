using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data.Migrations;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using Widgets.FirebasePushNotification.Components;
using Widgets.FirebasePushNotification.Models;

namespace Widgets.FirebasePushNotification;

public class FirebasePushNotificationPlugin : BasePlugin, IWidgetPlugin, IPlugin, IConsumer<ThirdPartyPluginsMenuItemCreatedEvent>
{
	private readonly IWebHelper _webHelper;

	private readonly ISettingService _settingService;

	private readonly ILocalizationService _localizationService;

	private readonly IMigrationManager _migrationManager;

	public bool HideInWidgetList => false;

	public FirebasePushNotificationPlugin(IWebHelper webHelper, ISettingService settingService, ILocalizationService localizationService, IMigrationManager migrationManager)
	{
		_webHelper = webHelper;
		_settingService = settingService;
		_localizationService = localizationService;
		_migrationManager = migrationManager;
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		return Task.FromResult((IList<string>)new List<string> { PublicWidgetZones.BodyEndHtmlTagBefore });
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		return typeof(FirebaseScriptViewComponent);
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/FirebasePushNotification/Configure";
	}

	public override async Task InstallAsync()
	{
		_migrationManager.ApplyUpMigrations(Assembly.GetExecutingAssembly());
		await _settingService.SaveSettingAsync(new FirebasePushNotificationSettings());
		await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
		{
			["Plugins.Widgets.FirebasePushNotification.Fields.ApiKey"] = "API Key",
			["Plugins.Widgets.FirebasePushNotification.Fields.AuthDomain"] = "Auth Domain",
			["Plugins.Widgets.FirebasePushNotification.Fields.ProjectId"] = "Project ID",
			["Plugins.Widgets.FirebasePushNotification.Fields.MessagingSenderId"] = "Messaging Sender ID",
			["Plugins.Widgets.FirebasePushNotification.Fields.AppId"] = "App ID",
			["Plugins.Widgets.FirebasePushNotification.Fields.VapidKey"] = "VAPID Key",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.PageTitle"] = "Send Broadcast Notification",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Custom"] = "Send Custom Notification",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Target"] = "Target",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SendToAll"] = "Send to all users",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SearchHint"] = "Search by username, first name, last name, or email.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Platform"] = "Platform",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.TitleEn"] = "Title (English)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.BodyEn"] = "Body (English)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.TitleAr"] = "Title (Arabic)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.BodyAr"] = "Body (Arabic)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.DataJson"] = "Data JSON",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Send"] = "Send Notification",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SearchPlaceholder"] = "Search and select a user",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Result"] = "Notification request processed for {0} user(s), sent to {1} device(s).",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Errors.TitleBodyRequired"] = "Please enter title and body in both English and Arabic.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Errors.SelectUser"] = "Please select a user or choose send to all users.",
			["Plugins.Widgets.FirebasePushNotification.Errors.InvalidDataJson"] = "Data JSON must be a valid string:string object.",
			["Plugins.Widgets.FirebasePushNotification.Test.Sent"] = "Test notification sent.",
			["Plugins.Widgets.FirebasePushNotification.Test.Failed"] = "Unable to send test notification."
		});
		//Arabic values for the broadcast page; the dictionary above seeds every
		//language with the English text, this overwrites ar-SA.
		foreach (var resource in new Dictionary<string, string>
		{
			["Plugins.Widgets.FirebasePushNotification.Broadcast.PageTitle"] = "إرسال إشعار جماعي",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Custom"] = "إرسال إشعار مخصص",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Target"] = "الجهة المستهدفة",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SendToAll"] = "إرسال إلى جميع المستخدمين",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SearchHint"] = "ابحث باسم المستخدم أو الاسم الأول أو اسم العائلة أو البريد الإلكتروني.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Platform"] = "المنصة",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.TitleEn"] = "العنوان (بالإنجليزية)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.BodyEn"] = "النص (بالإنجليزية)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.TitleAr"] = "العنوان (بالعربية)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.BodyAr"] = "النص (بالعربية)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.DataJson"] = "بيانات JSON",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Send"] = "إرسال الإشعار",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SearchPlaceholder"] = "ابحث واختر مستخدماً",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Result"] = "تمت معالجة طلب الإشعار لعدد {0} من المستخدمين، وتم الإرسال إلى {1} من الأجهزة.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Errors.TitleBodyRequired"] = "يرجى إدخال العنوان والنص باللغتين العربية والإنجليزية.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Errors.SelectUser"] = "يرجى اختيار مستخدم أو تحديد الإرسال إلى جميع المستخدمين.",
			["Plugins.Widgets.FirebasePushNotification.Errors.InvalidDataJson"] = "يجب أن تكون بيانات JSON كائناً صالحاً بقيم نصية.",
			["Plugins.Widgets.FirebasePushNotification.Test.Sent"] = "تم إرسال الإشعار التجريبي.",
			["Plugins.Widgets.FirebasePushNotification.Test.Failed"] = "تعذر إرسال الإشعار التجريبي."
		})
			await _localizationService.AddOrUpdateLocaleResourceAsync(resource.Key, resource.Value, "ar-SA");

		await base.InstallAsync();
	}

	public override async Task UninstallAsync()
	{
		await _settingService.DeleteSettingAsync<FirebasePushNotificationSettings>();
		_migrationManager.ApplyDownMigrations(Assembly.GetExecutingAssembly());
		await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.FirebasePushNotification");
		await base.UninstallAsync();
	}

	public async Task HandleEventAsync(ThirdPartyPluginsMenuItemCreatedEvent eventMessage)
	{
		AdminMenuItem pluginMenuItem = new AdminMenuItem
		{
			SystemName = "Widgets.FirebasePushNotification.Menu.SendBroadcast",
			Title = await _localizationService.GetResourceAsync("Plugins.Widgets.FirebasePushNotification.Broadcast.PageTitle"),
			IconClass = "far fa-bell",
			Url = _webHelper.GetStoreLocation() + "Admin/FirebasePushNotification/SendBroadcast",
			PermissionNames = new List<string>(1) { "Configuration.ManageWidgets" }
		};
		AdminMenuItem thirdPartyPluginsNode = eventMessage.MenuItem;
		if (thirdPartyPluginsNode != null && !thirdPartyPluginsNode.ContainsSystemName(pluginMenuItem.SystemName))
		{
			thirdPartyPluginsNode.ChildNodes.Add(pluginMenuItem);
		}
		await Task.CompletedTask;
	}
}
