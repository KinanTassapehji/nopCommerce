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

public class FirebasePushNotificationPlugin : BasePlugin, IWidgetPlugin, IPlugin, IConsumer<AdminMenuCreatedEvent>
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
		await AddResourcesAsync();

		await base.InstallAsync();
	}

	public override async Task UpdateAsync(string currentVersion, string targetVersion)
	{
		//4.80.1: Arabic text reached ar-SA only, so TmTm (ar-SY) got English; the Data JSON box became a Link field
		//4.80.2: the Arabic menu entry and page title are just "الإشعارات"
		await AddResourcesAsync();
		await base.UpdateAsync(currentVersion, targetVersion);
	}

	//English text for every language first, then the Arabic one overwritten -
	//ar-SA in Arabia, ar-SY in TmTm.
	private async Task AddResourcesAsync()
	{
		await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
		{
			["Plugins.Widgets.FirebasePushNotification.Fields.ApiKey"] = "API Key",
			["Plugins.Widgets.FirebasePushNotification.Fields.AuthDomain"] = "Auth Domain",
			["Plugins.Widgets.FirebasePushNotification.Fields.ProjectId"] = "Project ID",
			["Plugins.Widgets.FirebasePushNotification.Fields.MessagingSenderId"] = "Messaging Sender ID",
			["Plugins.Widgets.FirebasePushNotification.Fields.AppId"] = "App ID",
			["Plugins.Widgets.FirebasePushNotification.Fields.VapidKey"] = "VAPID Key",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.PageTitle"] = "Push notifications",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Custom"] = "Send Custom Notification",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Target"] = "Target",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SendToAll"] = "Send to all users",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SearchHint"] = "Search by username, first name, last name, or email.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Platform"] = "Platform",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.TitleEn"] = "Title (English)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.BodyEn"] = "Body (English)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.TitleAr"] = "Title (Arabic)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.BodyAr"] = "Body (Arabic)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Link"] = "Link to open (optional)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.LinkHint"] = "The page that opens when the notification is tapped. Open it on the store, copy the address from the browser and paste it here. Leave empty for the home page.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Send"] = "Send Notification",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SearchPlaceholder"] = "Search and select a user",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.InputTooShort"] = "Please enter 2 or more characters",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.NoResults"] = "No users found",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Searching"] = "Searching...",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Result"] = "Notification request processed for {0} user(s), sent to {1} device(s).",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Errors.TitleBodyRequired"] = "Please enter title and body in both English and Arabic.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Errors.SelectUser"] = "Please select a user or choose send to all users.",
			["Plugins.Widgets.FirebasePushNotification.Errors.InvalidDataJson"] = "Data JSON must be a valid string:string object.",
			["Plugins.Widgets.FirebasePushNotification.Test.Sent"] = "Test notification sent.",
			["Plugins.Widgets.FirebasePushNotification.Test.Failed"] = "Unable to send test notification."
		});
		foreach (var resource in new Dictionary<string, string>
		{
			["Plugins.Widgets.FirebasePushNotification.Broadcast.PageTitle"] = "الإشعارات الفورية",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Custom"] = "إرسال إشعار مخصص",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Target"] = "الجهة المستهدفة",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SendToAll"] = "إرسال إلى جميع المستخدمين",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SearchHint"] = "ابحث باسم المستخدم أو الاسم الأول أو اسم العائلة أو البريد الإلكتروني.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Platform"] = "المنصة",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.TitleEn"] = "العنوان (بالإنجليزية)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.BodyEn"] = "النص (بالإنجليزية)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.TitleAr"] = "العنوان (بالعربية)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.BodyAr"] = "النص (بالعربية)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Link"] = "الرابط عند الضغط (اختياري)",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.LinkHint"] = "الصفحة التي تُفتح عند الضغط على الإشعار. افتحها في المتجر وانسخ عنوانها من المتصفح والصقه هنا. اتركه فارغاً لفتح الصفحة الرئيسية.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Send"] = "إرسال الإشعار",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.SearchPlaceholder"] = "ابحث واختر مستخدماً",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.InputTooShort"] = "يرجى إدخال حرفين أو أكثر",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.NoResults"] = "لا يوجد مستخدمون مطابقون",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Searching"] = "جارٍ البحث...",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Result"] = "تمت معالجة طلب الإشعار لعدد {0} من المستخدمين، وتم الإرسال إلى {1} من الأجهزة.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Errors.TitleBodyRequired"] = "يرجى إدخال العنوان والنص باللغتين العربية والإنجليزية.",
			["Plugins.Widgets.FirebasePushNotification.Broadcast.Errors.SelectUser"] = "يرجى اختيار مستخدم أو تحديد الإرسال إلى جميع المستخدمين.",
			["Plugins.Widgets.FirebasePushNotification.Errors.InvalidDataJson"] = "يجب أن تكون بيانات JSON كائناً صالحاً بقيم نصية.",
			["Plugins.Widgets.FirebasePushNotification.Test.Sent"] = "تم إرسال الإشعار التجريبي.",
			["Plugins.Widgets.FirebasePushNotification.Test.Failed"] = "تعذر إرسال الإشعار التجريبي."
		})
		foreach (var culture in new[] { "ar-SA", "ar-SY" }) //a culture the store lacks is skipped
			await _localizationService.AddOrUpdateLocaleResourceAsync(resource.Key, resource.Value, culture);
	}

	public override async Task UninstallAsync()
	{
		await _settingService.DeleteSettingAsync<FirebasePushNotificationSettings>();
		_migrationManager.ApplyDownMigrations(Assembly.GetExecutingAssembly());
		await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.FirebasePushNotification");
		await base.UninstallAsync();
	}

	public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
	{
		AdminMenuItem pluginMenuItem = new AdminMenuItem
		{
			SystemName = "Widgets.FirebasePushNotification.Menu.SendBroadcast",
			Title = await _localizationService.GetResourceAsync("Plugins.Widgets.FirebasePushNotification.Broadcast.PageTitle"),
			IconClass = "far fa-bell",
			Url = _webHelper.GetStoreLocation() + "Admin/FirebasePushNotification/SendBroadcast",
			PermissionNames = new List<string>(1) { "Configuration.ManageWidgets" }
		};
		//under Marketing, not Plugins: a broadcast is a campaign, next to discounts
		var marketingNode = eventMessage.RootMenuItem.ChildNodes.FirstOrDefault(node => node.SystemName == "Marketing");
		if (marketingNode != null && !marketingNode.ContainsSystemName(pluginMenuItem.SystemName))
		{
			if (!marketingNode.InsertAfter("Discounts", pluginMenuItem))
				marketingNode.ChildNodes.Add(pluginMenuItem);
		}
		await Task.CompletedTask;
	}
}
