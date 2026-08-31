using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Widgets.FirebasePushNotification.Models;
using Widgets.FirebasePushNotification.Services;

namespace Widgets.FirebasePushNotification.Controllers;

[AuthorizeAdmin(false)]
[Area("Admin")]
[AutoValidateAntiforgeryToken]
public class FirebasePushNotificationController : BasePluginController
{
	private readonly ISettingService _settingService;

	private readonly INotificationService _notificationService;

	private readonly ILocalizationService _localizationService;

	private readonly IFirebaseNotificationService _firebaseNotificationService;

	private readonly ICustomerService _customerService;

	private readonly ILanguageService _languageService;

	public FirebasePushNotificationController(ISettingService settingService, INotificationService notificationService, ILocalizationService localizationService, IFirebaseNotificationService firebaseNotificationService, ICustomerService customerService, ILanguageService languageService)
	{
		_settingService = settingService;
		_notificationService = notificationService;
		_localizationService = localizationService;
		_firebaseNotificationService = firebaseNotificationService;
		_customerService = customerService;
		_languageService = languageService;
	}

	[CheckPermission("Configuration.ManageWidgets", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		FirebasePushNotificationSettings settings = await _settingService.LoadSettingAsync<FirebasePushNotificationSettings>();
		ConfigurationModel model = new ConfigurationModel
		{
			ApiKey = settings.ApiKey,
			AuthDomain = settings.AuthDomain,
			ProjectId = settings.ProjectId,
			MessagingSenderId = settings.MessagingSenderId,
			AppId = settings.AppId,
			VapidKey = settings.VapidKey
		};
		return View("~/Plugins/Widgets.FirebasePushNotification/Views/Configure.cshtml", model);
	}

	[HttpPost]
	[CheckPermission("Configuration.ManageWidgets", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		FirebasePushNotificationSettings settings = await _settingService.LoadSettingAsync<FirebasePushNotificationSettings>();
		settings.ApiKey = model.ApiKey?.Trim() ?? string.Empty;
		settings.AuthDomain = model.AuthDomain?.Trim() ?? string.Empty;
		settings.ProjectId = model.ProjectId?.Trim() ?? string.Empty;
		settings.MessagingSenderId = model.MessagingSenderId?.Trim() ?? string.Empty;
		settings.AppId = model.AppId?.Trim() ?? string.Empty;
		settings.VapidKey = model.VapidKey?.Trim() ?? string.Empty;
		await _settingService.SaveSettingAsync(settings);
		INotificationService notificationService = _notificationService;
		notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
		return await Configure();
	}

	[HttpGet]
	[CheckPermission("Configuration.ManageWidgets", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public IActionResult SendBroadcast()
	{
		ConfigurationModel configurationModel = new ConfigurationModel();
		int num = 4;
		List<SelectListItem> list = new List<SelectListItem>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<SelectListItem> span = CollectionsMarshal.AsSpan(list);
		span[0] = new SelectListItem("All", "all");
		span[1] = new SelectListItem("Android", "android");
		span[2] = new SelectListItem("iOS", "ios");
		span[3] = new SelectListItem("Web", "web");
		configurationModel.AvailablePlatforms = list;
		ConfigurationModel model = configurationModel;
		return View("~/Plugins/Widgets.FirebasePushNotification/Views/SendBroadcast.cshtml", model);
	}

	[HttpGet]
	[CheckPermission("Configuration.ManageWidgets", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> SearchCustomers(string term)
	{
		if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
		{
			return Json(Array.Empty<object>());
		}
		term = term.Trim();
		ICustomerService customerService = _customerService;
		string email = term;
		bool? isActive = true;
		IPagedList<Customer> byEmail = await customerService.GetAllCustomersAsync(null, null, null, null, 0, 0, null, email, null, null, null, 0, 0, null, null, null, null, isActive, 0, 10);
		ICustomerService customerService2 = _customerService;
		email = term;
		isActive = true;
		IPagedList<Customer> byUsername = await customerService2.GetAllCustomersAsync(null, null, null, null, 0, 0, null, null, email, null, null, 0, 0, null, null, null, null, isActive, 0, 10);
		ICustomerService customerService3 = _customerService;
		email = term;
		isActive = true;
		IPagedList<Customer> byFirstName = await customerService3.GetAllCustomersAsync(null, null, null, null, 0, 0, null, null, null, email, null, 0, 0, null, null, null, null, isActive, 0, 10);
		ICustomerService customerService4 = _customerService;
		email = term;
		isActive = true;
		var customers = (from customer in (from c in Enumerable.Concat(second: await customerService4.GetAllCustomersAsync(null, null, null, null, 0, 0, null, null, null, null, email, 0, 0, null, null, null, null, isActive, 0, 10), first: byEmail.Concat(byUsername).Concat(byFirstName))
				where c != null && c.Active && !c.Deleted
				group c by c.Id into g
				select g.First()).Take(20)
			select new
			{
				id = customer.Id,
				text = FormatCustomerDisplayName(customer)
			}).ToList();
		return Json(customers);
	}

	[HttpPost]
	[CheckPermission("Configuration.ManageWidgets", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> SendTest(SendTestRequest request)
	{
		Dictionary<string, string> data = ParseDataJson(request.DataJson);
		if (data == null && !string.IsNullOrWhiteSpace(request.DataJson))
		{
			_notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.FirebasePushNotification.Errors.InvalidDataJson"));
			return await Configure();
		}
		if (await _firebaseNotificationService.SendNotificationAsync(request.CustomerId, request.Title, request.Body, data, request.Platform))
		{
			_notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.FirebasePushNotification.Test.Sent"));
		}
		else
		{
			_notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.FirebasePushNotification.Test.Failed"));
		}
		return await Configure();
	}

	[HttpPost]
	[CheckPermission("Configuration.ManageWidgets", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> SendBroadcast(SendBroadcastRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.TitleEn) || string.IsNullOrWhiteSpace(request.BodyEn) || string.IsNullOrWhiteSpace(request.TitleAr) || string.IsNullOrWhiteSpace(request.BodyAr))
		{
			_notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.FirebasePushNotification.Broadcast.Errors.TitleBodyRequired"));
			return SendBroadcast();
		}
		Dictionary<string, string> data = ParseDataJson(request.DataJson);
		if (data == null && !string.IsNullOrWhiteSpace(request.DataJson))
		{
			_notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.FirebasePushNotification.Errors.InvalidDataJson"));
			return SendBroadcast();
		}
		List<int> targetCustomerIds = new List<int>();
		if (request.SendToAllUsers)
		{
			ICustomerService customerService = _customerService;
			bool? isActive = true;
			targetCustomerIds = (from customer in await customerService.GetAllCustomersAsync(null, null, null, null, 0, 0, null, null, null, null, null, 0, 0, null, null, null, null, isActive)
				where !customer.Deleted
				select customer.Id).Distinct().ToList();
		}
		else if (request.CustomerId > 0)
		{
			targetCustomerIds.Add(request.CustomerId);
		}
		if (!targetCustomerIds.Any())
		{
			_notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.FirebasePushNotification.Broadcast.Errors.SelectUser"));
			return SendBroadcast();
		}
		IList<Customer> customersByIds = await _customerService.GetCustomersByIdsAsync(targetCustomerIds.ToArray());
		Dictionary<int, string> languageMap = (await _languageService.GetAllLanguagesAsync(showHidden: true)).ToDictionary((Language language) => language.Id, (Language language) => language.LanguageCulture?.ToLowerInvariant() ?? string.Empty);
		List<int> arabicCustomerIds = (from customer in customersByIds.Where(delegate(Customer customer)
			{
				int? num3 = customer?.LanguageId;
				int result;
				if (num3.HasValue)
				{
					int valueOrDefault = num3.GetValueOrDefault();
					if (languageMap.TryGetValue(valueOrDefault, out var value))
					{
						result = (value.StartsWith("ar") ? 1 : 0);
						goto IL_0044;
					}
				}
				result = 0;
				goto IL_0044;
				IL_0044:
				return (byte)result != 0;
			})
			select customer.Id).ToList();
		List<int> englishCustomerIds = (from customer in customersByIds
			where customer != null && !arabicCustomerIds.Contains(customer.Id)
			select customer.Id).ToList();
		int sentCount = 0;
		if (englishCustomerIds.Any())
		{
			int num = sentCount;
			sentCount = num + await _firebaseNotificationService.SendNotificationToManyAsync(englishCustomerIds, request.TitleEn, request.BodyEn, data, request.Platform);
		}
		if (arabicCustomerIds.Any())
		{
			int num2 = sentCount;
			sentCount = num2 + await _firebaseNotificationService.SendNotificationToManyAsync(arabicCustomerIds, request.TitleAr, request.BodyAr, data, request.Platform);
		}
		_notificationService.SuccessNotification(string.Format(
			await _localizationService.GetResourceAsync("Plugins.Widgets.FirebasePushNotification.Broadcast.Result"),
			targetCustomerIds.Count, sentCount));
		return RedirectToAction("SendBroadcast");
	}

	private static string FormatCustomerDisplayName(Customer customer)
	{
		string text = (customer.FirstName + " " + customer.LastName).Trim();
		string text2 = (string.IsNullOrWhiteSpace(text) ? customer.Email : text);
		return text2 + " (" + (customer.Username ?? customer.Email ?? customer.Id.ToString()) + ")";
	}

	private static Dictionary<string, string>? ParseDataJson(string dataJson)
	{
		if (string.IsNullOrWhiteSpace(dataJson))
		{
			return null;
		}
		try
		{
			return JsonSerializer.Deserialize<Dictionary<string, string>>(dataJson);
		}
		catch (JsonException)
		{
			return null;
		}
	}
}
