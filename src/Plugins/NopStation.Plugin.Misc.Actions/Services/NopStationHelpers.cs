using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;

namespace NopStation.Plugin.Misc.Core.Services;

public static class NopStationHelpers
{
	private static async Task<PermissionRecord> GetPermissionRecordBySystemNameAsync(string systemName, IRepository<PermissionRecord> permissionRecordRepository)
	{
		if (string.IsNullOrWhiteSpace(systemName))
		{
			return null;
		}
		return await (from pr in permissionRecordRepository.Table
			where pr.SystemName == systemName
			orderby pr.Id
			select pr).FirstOrDefaultAsync();
	}

	private static async Task DeletePermissionRecordAsync(PermissionRecord permission, IRepository<PermissionRecord> permissionRecordRepository)
	{
		await permissionRecordRepository.DeleteAsync(permission);
	}

	public static async Task InstallPluginAsync<TPlugin>(this TPlugin plugin, bool autoEnable = true) where TPlugin : class, INopStationPlugin
	{
		IEngine engine = EngineContext.Current;
		ISettingService settingService = engine.Resolve<ISettingService>();
		engine.Resolve<ILocalizationService>().AddOrUpdateLocaleResource(plugin.GetPluginResources());
		NopStationCoreSettings nopStationCoreSettings = engine.Resolve<NopStationCoreSettings>();
		List<string> activeNopStationSystemNames = nopStationCoreSettings.ActiveNopStationSystemNames;
		if (activeNopStationSystemNames != null && !activeNopStationSystemNames.Any((string x) => x == plugin.PluginDescriptor.SystemName))
		{
			nopStationCoreSettings.ActiveNopStationSystemNames.Add(plugin.PluginDescriptor.SystemName);
			await settingService.SaveSettingAsync(nopStationCoreSettings);
		}
		if (autoEnable)
		{
			await plugin.EnablePluginAsync(settingService);
		}
		await engine.Resolve<NopStationHttpClient>().OnInstallPluginAsync(plugin.PluginDescriptor);
	}

	public static async Task UninstallPluginAsync<TPlugin>(this TPlugin plugin, IPermissionConfigManager provider = null) where TPlugin : class, INopStationPlugin
	{
		IEngine engine = EngineContext.Current;
		await engine.Resolve<ILocalizationService>().DeleteLocaleResourcesAsync(plugin.GetPluginResources().Keys.ToList());
		if (provider != null)
		{
			engine.Resolve<IPermissionService>();
			IRepository<PermissionRecord> permissionRecordRepository = engine.Resolve<IRepository<PermissionRecord>>();
			foreach (PermissionConfig allConfig in provider.AllConfigs)
			{
				PermissionRecord permissionRecord = await GetPermissionRecordBySystemNameAsync(allConfig.SystemName, permissionRecordRepository);
				if (permissionRecord != null)
				{
					await DeletePermissionRecordAsync(permissionRecord, permissionRecordRepository);
				}
			}
		}
		NopStationCoreSettings nopStationCoreSettings = engine.Resolve<NopStationCoreSettings>();
		if (nopStationCoreSettings.ActiveNopStationSystemNames.Any((string x) => x == plugin.PluginDescriptor.SystemName))
		{
			ISettingService settingService = engine.Resolve<ISettingService>();
			nopStationCoreSettings.ActiveNopStationSystemNames.Remove(plugin.PluginDescriptor.SystemName);
			await settingService.SaveSettingAsync(nopStationCoreSettings);
		}
		await engine.Resolve<NopStationHttpClient>().OnUninstallPluginAsync(plugin.PluginDescriptor);
	}

	public static async Task EnablePluginAsync<TPlugin>(this TPlugin plugin, ISettingService settingService) where TPlugin : INopStationPlugin
	{
		IEngine engine = EngineContext.Current;
		try
		{
			if (!(plugin is IPaymentMethod paymentMethod))
			{
				if (!(plugin is IShippingRateComputationMethod shippingProvider))
				{
					if (!(plugin is IPickupPointProvider pickupPointProvider))
					{
						// ponytail: external authentication is not part of this fork
						if (plugin is IWidgetPlugin widget && !engine.Resolve<IWidgetPluginManager>().IsPluginActive(widget))
						{
							WidgetSettings widgetSettings = engine.Resolve<WidgetSettings>();
							widgetSettings.ActiveWidgetSystemNames.Add(plugin.PluginDescriptor.SystemName);
							await settingService.SaveSettingAsync(widgetSettings);
						}
					}
					else if (!engine.Resolve<IPickupPluginManager>().IsPluginActive(pickupPointProvider))
					{
						ShippingSettings shippingSettings = engine.Resolve<ShippingSettings>();
						shippingSettings.ActivePickupPointProviderSystemNames.Add(plugin.PluginDescriptor.SystemName);
						await settingService.SaveSettingAsync(shippingSettings);
					}
				}
				else if (!engine.Resolve<IShippingPluginManager>().IsPluginActive(shippingProvider))
				{
					ShippingSettings shippingSettings2 = engine.Resolve<ShippingSettings>();
					shippingSettings2.ActiveShippingRateComputationMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
					await settingService.SaveSettingAsync(shippingSettings2);
				}
			}
			else if (!engine.Resolve<IPaymentPluginManager>().IsPluginActive(paymentMethod))
			{
				PaymentSettings paymentSettings = engine.Resolve<PaymentSettings>();
				paymentSettings.ActivePaymentMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
				await settingService.SaveSettingAsync(paymentSettings);
			}
		}
		catch (Exception ex)
		{
			await engine.Resolve<ILogger>().ErrorAsync("Failed to enable " + plugin.PluginDescriptor.SystemName + ": " + ex.Message, ex);
		}
	}
}
