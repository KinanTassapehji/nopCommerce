using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Security;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Controllers;

public class NopStationCoreController : NopStationAdminController
{
	private readonly IStoreContext _storeContext;

	private readonly ILocalizationService _localizationService;

	private readonly IPermissionService _permissionService;

	private readonly IWorkContext _workContext;

	private readonly IBaseAdminModelFactory _baseAdminModelFactory;

	private readonly INopStationPluginManager _nopStationPluginManager;

	private readonly ICustomerService _customerService;

	private readonly INotificationService _notificationService;

	private readonly INopFileProvider _fileProvider;

	private readonly ISettingHelper<NopStationCoreSettings, ConfigurationModel> _settingHelper;

	public NopStationCoreController(IStoreContext storeContext, ILocalizationService localizationService, IPermissionService permissionService, IWorkContext workContext, IBaseAdminModelFactory baseAdminModelFactory, INopStationPluginManager nopStationPluginManager, ICustomerService customerService, INotificationService notificationService, INopFileProvider fileProvider, ISettingHelper<NopStationCoreSettings, ConfigurationModel> settingHelper)
	{
		_storeContext = storeContext;
		_localizationService = localizationService;
		_permissionService = permissionService;
		_workContext = workContext;
		_baseAdminModelFactory = baseAdminModelFactory;
		_nopStationPluginManager = nopStationPluginManager;
		_customerService = customerService;
		_notificationService = notificationService;
		_fileProvider = fileProvider;
		_settingHelper = settingHelper;
	}

	[CheckPermission("ManageNopStationCoreConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		ConfigurationModel model = await _settingHelper.PrepareConfigurationModelAsync(null);
		await _baseAdminModelFactory.PrepareCustomerRolesAsync(model.AvailableCustomerRoles, withSpecialDefaultItem: false);
		return View(model);
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationCoreConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		CustomerRole adminRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
		if (adminRole != null && !model.AllowedCustomerRoleIds.Contains(adminRole.Id))
		{
			INotificationService notificationService = _notificationService;
			notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.Configuration.AdminCanNotBeRestricted"));
			model.AllowedCustomerRoleIds.Add(adminRole.Id);
		}
		await _settingHelper.SaveConfigurationModelAsync(model, null, true);
		return RedirectToAction("Configure");
	}

	[CheckPermission("ManageNopStationCoreConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> LocaleResource()
	{
		CoreLocaleResourceSearchModel searchModel = new CoreLocaleResourceSearchModel();
		CoreLocaleResourceSearchModel coreLocaleResourceSearchModel = searchModel;
		coreLocaleResourceSearchModel.SearchLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id;
		await _baseAdminModelFactory.PrepareLanguagesAsync(searchModel.AvailableLanguages, withSpecialDefaultItem: false);
		foreach (INopStationPlugin item in await _nopStationPluginManager.LoadNopStationPluginsAsync(null, "", _storeContext.GetCurrentStoreAsync().Id))
		{
			searchModel.AvailablePlugins.Add(new SelectListItem
			{
				Value = item.PluginDescriptor.SystemName,
				Text = item.PluginDescriptor.FriendlyName
			});
		}
		searchModel.AvailablePlugins.Insert(0, new SelectListItem
		{
			Value = "",
			Text = _localizationService.GetResourceAsync("Admin.NopStation.Core.Resources.List.SearchPluginSystemName.All").Result
		});
		return View(searchModel);
	}

	[HttpPost]
	[CheckPermission("ManageNopStationCoreConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> LocaleResource(CoreLocaleResourceSearchModel searchModel)
	{
		IPagedList<(string Key, string Value)> resources = await _nopStationPluginManager.LoadPluginStringResourcesAsync(searchModel.SearchPluginSystemName, searchModel.SearchResourceName, searchModel.SearchLanguageId, _storeContext.GetCurrentStoreAsync().Id, searchModel.Page - 1, searchModel.PageSize);
		CoreLocaleResourceListModel data = new CoreLocaleResourceListModel().PrepareToGrid(searchModel, resources, () => resources.Select(((string Key, string Value) resource) => new CoreLocaleResourceModel
		{
			ResourceName = resource.Key.ToLower(),
			ResourceValue = resource.Value,
			ResourceNameLanguageId = $"{resource.Key}___{searchModel.SearchLanguageId}"
		}));
		return Json(data);
	}

	[EditAccessAjax(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationCoreConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<JsonResult> ResourceUpdate(CoreLocaleResourceModel model)
	{
		if (string.IsNullOrWhiteSpace(model.ResourceNameLanguageId))
		{
			return ErrorJson(_localizationService.GetResourceAsync("Admin.NopStation.Core.Resources.FailedToSave"));
		}
		string[] array = model.ResourceNameLanguageId.Split(new string[1] { "___" }, StringSplitOptions.None);
		model.ResourceName = array[0];
		model.LanguageId = int.Parse(array[1]);
		if (model.ResourceValue != null)
		{
			model.ResourceValue = model.ResourceValue.Trim();
		}
		LocaleStringResource result = _localizationService.GetLocaleStringResourceByNameAsync(model.ResourceName, model.LanguageId).Result;
		if (result != null)
		{
			result.ResourceValue = model.ResourceValue;
			await _localizationService.UpdateLocaleStringResourceAsync(result);
		}
		else
		{
			LocaleStringResource localeStringResource = model.ToEntity<LocaleStringResource>();
			localeStringResource.LanguageId = model.LanguageId;
			await _localizationService.InsertLocaleStringResourceAsync(localeStringResource);
		}
		return new NullJsonResult();
	}

	[CheckPermission("ManageNopStationCoreLicense", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> AssemblyInfo()
	{
		await _nopStationPluginManager.LoadNopStationPluginsAsync(null, "", _storeContext.GetCurrentStoreAsync().Id);
		List<PluginInfoModel> list = new List<PluginInfoModel>();
		foreach (Assembly item2 in (from x in AppDomain.CurrentDomain.GetAssemblies()
			where x.FullName.StartsWith("NopStation.Plugin") && !x.GetName().Name.EndsWith(".Views")
			select x).ToList())
		{
			AssemblyName name = item2.GetName();
			string text = (item2.IsDynamic ? null : item2.Location);
			object[] customAttributes = item2.GetCustomAttributes(inherit: false);
			AssemblyProductAttribute assemblyProductAttribute = customAttributes.FirstOrDefault((object x) => x.GetType() == typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
			AssemblyDescriptionAttribute assemblyDescriptionAttribute = customAttributes.FirstOrDefault((object x) => x.GetType() == typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
			string buildType = "";
			if (customAttributes.FirstOrDefault((object x) => x.GetType() == typeof(DebuggableAttribute)) is DebuggableAttribute debuggableAttribute)
			{
				buildType = (debuggableAttribute.IsJITOptimizerDisabled ? "Debug" : "Release");
			}
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(text);
			PluginInfoModel item = new PluginInfoModel
			{
				FileName = _fileProvider.GetFileName(text),
				FilePath = text,
				AssemblyVersion = ((name.Version == null) ? "" : name.Version.ToString()),
				AssemblyName = assemblyProductAttribute?.Product,
				CreatedOn = _fileProvider.GetCreationTime(text),
				BuildType = buildType,
				FileVersion = versionInfo?.FileVersion,
				Description = assemblyDescriptionAttribute?.Description
			};
			list.Add(item);
		}
		return View(list);
	}

	[CheckPermission("Configuration.ManageACL", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Permissions()
	{
		PermissionConfigurationModel model = new PermissionConfigurationModel
		{
			AreCustomerRolesAvailable = (await _customerService.GetAllCustomerRolesAsync(showHidden: true)).Any()
		};
		List<PermissionRecord> source = (await _permissionService.GetAllPermissionRecordsAsync()).Where((PermissionRecord x) => x.Category == "NopStation").ToList();
		model.IsPermissionsAvailable = source.Any();
		return View(model);
	}
}
