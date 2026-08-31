using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Areas.Admin.Factories;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Controllers;

public class NopStationMarketplaceController : NopStationAdminController
{
	private readonly ILocalizationService _localizationService;

	private readonly IMarketplaceService _marketplaceService;

	private readonly IPluginService _pluginService;

	private readonly IWorkContext _workContext;

	private readonly IMarketplaceModelFactory _marketplaceModelFactory;

	private readonly IWebHelper _webHelper;

	public NopStationMarketplaceController(ILocalizationService localizationService, IMarketplaceService marketplaceService, IPluginService pluginService, IWorkContext workContext, IMarketplaceModelFactory marketplaceModelFactory, IWebHelper webHelper)
	{
		_localizationService = localizationService;
		_marketplaceService = marketplaceService;
		_pluginService = pluginService;
		_workContext = workContext;
		_marketplaceModelFactory = marketplaceModelFactory;
		_webHelper = webHelper;
	}

	[CheckPermission("ManageNopStationCoreConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Products(MarketplaceSearchModel command)
	{
		return View(await _marketplaceModelFactory.PrepareMarketplaceListModelAsync(command));
	}

	[HttpGet]
	[CheckPermission("ManageNopStationCoreConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> GetProducts(MarketplaceSearchModel command)
	{
		return PartialView("_ProductsList", await _marketplaceModelFactory.PrepareMarketplaceListModelAsync(command));
	}

	[HttpPost]
	[CheckPermission("ManageNopStationCoreConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Install(int productId, string systemName)
	{
		if (productId <= 0)
		{
			return Json(new
			{
				success = false,
				message = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Marketplace.Install.InvalidDownloadUrl")
			});
		}
		IActionResult result = default(IActionResult);
		object obj;
		int num;
		try
		{
			if (!string.IsNullOrWhiteSpace(systemName) && await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(systemName) != null)
			{
				result = Json(new
				{
					success = false,
					message = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Marketplace.Install.AlreadyInstalled")
				});
				return result;
			}
			_webHelper.GetStoreLocation();
			IList<string> uploadedSystemNames = await _marketplaceService.DownloadAndInstallPluginAsync(productId, "4.90");
			if (uploadedSystemNames.Count == 0)
			{
				result = Json(new
				{
					success = false,
					message = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Marketplace.Install.UploadFailed")
				});
				return result;
			}
			Customer customer = await _workContext.GetCurrentCustomerAsync();
			foreach (string pluginSystemName in uploadedSystemNames)
			{
				PluginDescriptor pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(pluginSystemName, LoadPluginsMode.All);
				if (pluginDescriptor != null && !pluginDescriptor.Installed)
				{
					await _pluginService.PreparePluginToInstallAsync(pluginSystemName, customer);
				}
			}
			result = Json(new
			{
				success = true,
				message = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Marketplace.Install.RestartRequired")
			});
			return result;
		}
		catch (Exception ex)
		{
			obj = ex;
			num = 1;
		}
		if (num != 1)
		{
			return result;
		}
		return Json(new
		{
			success = false,
			message = string.Format(arg0: ((Exception)obj).Message, format: await _localizationService.GetResourceAsync("Admin.NopStation.Core.Marketplace.Install.Failed"))
		});
	}

	[HttpPost]
	[CheckPermission("ManageNopStationCoreConfiguration", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> UpgradeRequest(string systemName)
	{
		if (string.IsNullOrWhiteSpace(systemName))
		{
			return Json(new
			{
				success = false,
				message = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Marketplace.UpgradeRequest.InvalidSystemName")
			});
		}
		IActionResult result = default(IActionResult);
		object obj;
		int num;
		try
		{
			await _marketplaceService.SendUpgradeRequestAsync(systemName, "4.90");
			result = Json(new
			{
				success = true,
				message = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Marketplace.UpgradeRequest.Sent")
			});
			return result;
		}
		catch (Exception ex)
		{
			obj = ex;
			num = 1;
		}
		if (num != 1)
		{
			return result;
		}
		return Json(new
		{
			success = false,
			message = string.Format(arg0: ((Exception)obj).Message, format: await _localizationService.GetResourceAsync("Admin.NopStation.Core.Marketplace.UpgradeRequest.Failed"))
		});
	}
}
