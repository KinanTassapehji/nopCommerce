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
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using Widgets.FirebasePushNotification.Components;
using Widgets.FirebasePushNotification.Models;
using Widgets.FirebasePushNotification.Services;

namespace Widgets.FirebasePushNotification;

public class FirebasePushNotificationPlugin : BasePlugin, IWidgetPlugin, IPlugin, IConsumer<AdminMenuCreatedEvent>
{
	private readonly IWebHelper _webHelper;

	private readonly ISettingService _settingService;

	private readonly ILocalizationService _localizationService;

	private readonly IMigrationManager _migrationManager;

	private readonly ILanguageService _languageService;

	private readonly InboxNotificationService _inboxNotificationService;

	public bool HideInWidgetList => false;

	public FirebasePushNotificationPlugin(IWebHelper webHelper, ISettingService settingService, ILocalizationService localizationService, IMigrationManager migrationManager, ILanguageService languageService, InboxNotificationService inboxNotificationService)
	{
		_languageService = languageService;
		_inboxNotificationService = inboxNotificationService;
		_webHelper = webHelper;
		_settingService = settingService;
		_localizationService = localizationService;
		_migrationManager = migrationManager;
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		return Task.FromResult((IList<string>)new List<string>
		{
			PublicWidgetZones.BodyEndHtmlTagBefore,
			PublicWidgetZones.HeaderLinksBefore,
			PublicWidgetZones.AccountNavigationBefore,
			AdminWidgetZones.HeaderMiddle
		});
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		return widgetZone == PublicWidgetZones.BodyEndHtmlTagBefore ? typeof(FirebaseScriptViewComponent) : typeof(InboxLinkViewComponent);
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
		//4.80.3: notifications pages (account + admin inbox); order pushes in the customer's language
		//4.80.4: an admin notification when a customer closes their own account
		//4.80.5: admin notifications are written in the reading admin's language; rows stored as
		//        English text until now become key + arguments
		//4.80.6: the admin inbox's filter tabs
		await AddResourcesAsync();
		await ConvertAdminTextRowsAsync();
		await base.UpdateAsync(currentVersion, targetVersion);
	}

	private static readonly string[] AdminMessageKeys = { "NewOrder", "OrderCancelled", "NewCustomer", "NewReview", "LowStock", "AccountClosed" };

	private async Task ConvertAdminTextRowsAsync()
	{
		//the rows were written in the default admin language, which has been English on both stores
		var english = (await _languageService.GetAllLanguagesAsync(showHidden: true))
			.FirstOrDefault(language => language.LanguageCulture.StartsWith("en", StringComparison.OrdinalIgnoreCase));
		if (english == null)
			return;

		var messages = new Dictionary<string, (string Title, string Body)>();
		foreach (var key in AdminMessageKeys)
			messages[key] = (await _localizationService.GetResourceAsync(InboxNotificationService.AdminMessagePrefix + key + ".Title", english.Id),
				await _localizationService.GetResourceAsync(InboxNotificationService.AdminMessagePrefix + key + ".Body", english.Id));
		await _inboxNotificationService.ConvertAdminTextRowsAsync(messages);
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
			["Plugins.Widgets.FirebasePushNotification.Test.Failed"] = "Unable to send test notification.",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Title"] = "Notifications",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Empty"] = "You have no notifications yet.",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Open"] = "View details",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Filter.All"] = "All",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Filter.orders"] = "Orders",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Filter.customers"] = "Customers",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Filter.reviews"] = "Reviews",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Filter.stock"] = "Stock",
			["Plugins.Widgets.FirebasePushNotification.Order.Placed.Title"] = "Order placed",
			["Plugins.Widgets.FirebasePushNotification.Order.Placed.Body"] = "Your order #{0} has been placed.",
			["Plugins.Widgets.FirebasePushNotification.Order.Processing.Title"] = "Order processing",
			["Plugins.Widgets.FirebasePushNotification.Order.Processing.Body"] = "Your order #{0} is being processed.",
			["Plugins.Widgets.FirebasePushNotification.Order.Complete.Title"] = "Order complete",
			["Plugins.Widgets.FirebasePushNotification.Order.Complete.Body"] = "Your order #{0} has been completed.",
			["Plugins.Widgets.FirebasePushNotification.Order.Cancelled.Title"] = "Order cancelled",
			["Plugins.Widgets.FirebasePushNotification.Order.Cancelled.Body"] = "Your order #{0} has been cancelled.",
			["Plugins.Widgets.FirebasePushNotification.Order.Paid.Title"] = "Payment confirmed",
			["Plugins.Widgets.FirebasePushNotification.Order.Paid.Body"] = "Payment for order #{0} has been confirmed.",
			["Plugins.Widgets.FirebasePushNotification.Order.Shipped.Title"] = "Order shipped",
			["Plugins.Widgets.FirebasePushNotification.Order.Shipped.Body"] = "Your order #{0} has been shipped.",
			["Plugins.Widgets.FirebasePushNotification.Order.Delivered.Title"] = "Order delivered",
			["Plugins.Widgets.FirebasePushNotification.Order.Delivered.Body"] = "Your order #{0} has been delivered.",
			["Plugins.Widgets.FirebasePushNotification.Order.ReadyForPickup.Title"] = "Ready for pickup",
			["Plugins.Widgets.FirebasePushNotification.Order.ReadyForPickup.Body"] = "Your order #{0} is ready for pickup.",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewOrder.Title"] = "New order",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewOrder.Body"] = "Order #{0} from {1}, total {2}.",
			["Plugins.Widgets.FirebasePushNotification.Admin.OrderCancelled.Title"] = "Order cancelled",
			["Plugins.Widgets.FirebasePushNotification.Admin.OrderCancelled.Body"] = "Order #{0} has been cancelled.",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewCustomer.Title"] = "New customer",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewCustomer.Body"] = "{0} ({1}) has registered.",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewReview.Title"] = "New product review",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewReview.Body"] = "{0} was rated {1}/5.",
			["Plugins.Widgets.FirebasePushNotification.Admin.LowStock.Title"] = "Low stock",
			["Plugins.Widgets.FirebasePushNotification.Admin.LowStock.Body"] = "{0} is down to {1} in stock.",
			["Plugins.Widgets.FirebasePushNotification.Admin.AccountClosed.Title"] = "Account closed by the customer",
			["Plugins.Widgets.FirebasePushNotification.Admin.AccountClosed.Body"] = "{0} ({1}) closed their account. It is deactivated; orders and details are kept."
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
			["Plugins.Widgets.FirebasePushNotification.Test.Failed"] = "تعذر إرسال الإشعار التجريبي.",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Title"] = "الإشعارات",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Empty"] = "لا توجد لديك إشعارات بعد.",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Open"] = "عرض التفاصيل",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Filter.All"] = "الكل",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Filter.orders"] = "الطلبات",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Filter.customers"] = "العملاء",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Filter.reviews"] = "التقييمات",
			["Plugins.Widgets.FirebasePushNotification.Inbox.Filter.stock"] = "المخزون",
			["Plugins.Widgets.FirebasePushNotification.Order.Placed.Title"] = "تم استلام الطلب",
			["Plugins.Widgets.FirebasePushNotification.Order.Placed.Body"] = "تم تقديم طلبك رقم {0} بنجاح.",
			["Plugins.Widgets.FirebasePushNotification.Order.Processing.Title"] = "الطلب قيد التجهيز",
			["Plugins.Widgets.FirebasePushNotification.Order.Processing.Body"] = "طلبك رقم {0} قيد التجهيز الآن.",
			["Plugins.Widgets.FirebasePushNotification.Order.Complete.Title"] = "اكتمل الطلب",
			["Plugins.Widgets.FirebasePushNotification.Order.Complete.Body"] = "اكتمل طلبك رقم {0}.",
			["Plugins.Widgets.FirebasePushNotification.Order.Cancelled.Title"] = "تم إلغاء الطلب",
			["Plugins.Widgets.FirebasePushNotification.Order.Cancelled.Body"] = "تم إلغاء طلبك رقم {0}.",
			["Plugins.Widgets.FirebasePushNotification.Order.Paid.Title"] = "تم تأكيد الدفع",
			["Plugins.Widgets.FirebasePushNotification.Order.Paid.Body"] = "تم تأكيد الدفع للطلب رقم {0}.",
			["Plugins.Widgets.FirebasePushNotification.Order.Shipped.Title"] = "تم شحن الطلب",
			["Plugins.Widgets.FirebasePushNotification.Order.Shipped.Body"] = "تم شحن طلبك رقم {0}.",
			["Plugins.Widgets.FirebasePushNotification.Order.Delivered.Title"] = "تم توصيل الطلب",
			["Plugins.Widgets.FirebasePushNotification.Order.Delivered.Body"] = "تم توصيل طلبك رقم {0}.",
			["Plugins.Widgets.FirebasePushNotification.Order.ReadyForPickup.Title"] = "جاهز للاستلام",
			["Plugins.Widgets.FirebasePushNotification.Order.ReadyForPickup.Body"] = "طلبك رقم {0} جاهز للاستلام.",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewOrder.Title"] = "طلب جديد",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewOrder.Body"] = "الطلب رقم {0} من {1}، الإجمالي {2}.",
			["Plugins.Widgets.FirebasePushNotification.Admin.OrderCancelled.Title"] = "تم إلغاء طلب",
			["Plugins.Widgets.FirebasePushNotification.Admin.OrderCancelled.Body"] = "تم إلغاء الطلب رقم {0}.",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewCustomer.Title"] = "عميل جديد",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewCustomer.Body"] = "سجّل {0} ({1}) حساباً جديداً.",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewReview.Title"] = "تقييم منتج جديد",
			["Plugins.Widgets.FirebasePushNotification.Admin.NewReview.Body"] = "حصل {0} على تقييم {1}/5.",
			["Plugins.Widgets.FirebasePushNotification.Admin.LowStock.Title"] = "مخزون منخفض",
			["Plugins.Widgets.FirebasePushNotification.Admin.LowStock.Body"] = "بقي من {0} في المخزون {1} فقط.",
			["Plugins.Widgets.FirebasePushNotification.Admin.AccountClosed.Title"] = "أغلق عميل حسابه",
			["Plugins.Widgets.FirebasePushNotification.Admin.AccountClosed.Body"] = "أغلق {0} ({1}) حسابه بنفسه. تم تعطيل الحساب مع الاحتفاظ بالطلبات والبيانات."
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
		//top of the menu, under the dashboard: the inbox is where the day starts
		var inboxMenuItem = new AdminMenuItem
		{
			SystemName = "Widgets.FirebasePushNotification.Menu.Inbox",
			Title = await _localizationService.GetResourceAsync("Plugins.Widgets.FirebasePushNotification.Inbox.Title"),
			IconClass = "far fa-bell",
			Url = _webHelper.GetStoreLocation() + "Admin/FirebasePushNotification/Inbox",
			PermissionNames = new List<string> { StandardPermission.Orders.ORDERS_VIEW }
		};
		if (!eventMessage.RootMenuItem.ContainsSystemName(inboxMenuItem.SystemName))
			eventMessage.RootMenuItem.ChildNodes.Insert(Math.Min(1, eventMessage.RootMenuItem.ChildNodes.Count), inboxMenuItem);

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
