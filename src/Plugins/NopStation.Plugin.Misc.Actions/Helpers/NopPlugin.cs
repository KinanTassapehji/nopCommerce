using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;

namespace NopStation.Plugin.Misc.Core.Helpers;

public static class NopPlugin
{
	public static async Task<bool> IsEnabledAsync<TPlugin>(string systemName) where TPlugin : class, IPlugin
	{
		IEngine engine = EngineContext.Current;
		try
		{
			IPluginService pluginService = engine.Resolve<IPluginService>();
			IStoreContext storeContext = engine.Resolve<IStoreContext>();
			IWorkContext workContext = engine.Resolve<IWorkContext>();
			IPluginService pluginService2 = pluginService;
			PluginDescriptor pluginDescriptor = await pluginService2.GetPluginDescriptorBySystemNameAsync<TPlugin>(systemName, LoadPluginsMode.InstalledOnly, await workContext.GetCurrentCustomerAsync(), storeContext.GetCurrentStore().Id);
			if (pluginDescriptor == null)
			{
				return false;
			}
			IPlugin plugin = pluginDescriptor.Instance<IPlugin>();
			if (!(plugin is IPaymentMethod paymentMethod))
			{
				if (!(plugin is IShippingRateComputationMethod shippingProvider))
				{
					if (!(plugin is IPickupPointProvider pickupPointProvider))
					{
						// ponytail: external authentication is not part of this fork
						if (plugin is IWidgetPlugin widget)
						{
							return engine.Resolve<IWidgetPluginManager>().IsPluginActive(widget);
						}
						return false;
					}
					return engine.Resolve<IPickupPluginManager>().IsPluginActive(pickupPointProvider);
				}
				return engine.Resolve<IShippingPluginManager>().IsPluginActive(shippingProvider);
			}
			return engine.Resolve<IPaymentPluginManager>().IsPluginActive(paymentMethod);
		}
		catch (Exception ex)
		{
			await engine.Resolve<ILogger>().ErrorAsync("Failed to check " + systemName + ": " + ex.Message, ex);
			return false;
		}
	}

	public static async Task EnablePlugin(this IPlugin plugin, PluginType pluginType)
	{
		IEngine engine = EngineContext.Current;
		try
		{
			ISettingService settingService = engine.Resolve<ISettingService>();
			switch (pluginType)
			{
			case PluginType.PaymentMethod:
			{
				PaymentSettings paymentSettings = engine.Resolve<PaymentSettings>();
				if (!paymentSettings.ActivePaymentMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
				{
					paymentSettings.ActivePaymentMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
					await settingService.SaveSettingAsync(paymentSettings);
				}
				break;
			}
			case PluginType.ShippingRateComputationMethod:
			{
				ShippingSettings shippingSettings2 = engine.Resolve<ShippingSettings>();
				if (!shippingSettings2.ActiveShippingRateComputationMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
				{
					shippingSettings2.ActiveShippingRateComputationMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
					await settingService.SaveSettingAsync(shippingSettings2);
				}
				break;
			}
			case PluginType.PickupPointProvider:
			{
				ShippingSettings shippingSettings = engine.Resolve<ShippingSettings>();
				if (!shippingSettings.ActivePickupPointProviderSystemNames.Contains(plugin.PluginDescriptor.SystemName))
				{
					shippingSettings.ActivePickupPointProviderSystemNames.Add(plugin.PluginDescriptor.SystemName);
					await settingService.SaveSettingAsync(shippingSettings);
				}
				break;
			}
			case PluginType.WidgetPlugin:
			{
				WidgetSettings widgetSettings = engine.Resolve<WidgetSettings>();
				if (!widgetSettings.ActiveWidgetSystemNames.Contains(plugin.PluginDescriptor.SystemName))
				{
					widgetSettings.ActiveWidgetSystemNames.Add(plugin.PluginDescriptor.SystemName);
					await settingService.SaveSettingAsync(widgetSettings);
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			await engine.Resolve<ILogger>().ErrorAsync("Failed to enable " + plugin.PluginDescriptor.SystemName + ": " + ex.Message, ex);
		}
	}

	public static async Task DisablePlugin(this IPlugin plugin, PluginType pluginType)
	{
		IEngine engine = EngineContext.Current;
		try
		{
			ISettingService settingService = engine.Resolve<ISettingService>();
			switch (pluginType)
			{
			case PluginType.PaymentMethod:
			{
				PaymentSettings paymentSettings = engine.Resolve<PaymentSettings>();
				if (paymentSettings.ActivePaymentMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
				{
					paymentSettings.ActivePaymentMethodSystemNames.Remove(plugin.PluginDescriptor.SystemName);
					await settingService.SaveSettingAsync(paymentSettings);
				}
				break;
			}
			case PluginType.ShippingRateComputationMethod:
			{
				ShippingSettings shippingSettings2 = engine.Resolve<ShippingSettings>();
				if (shippingSettings2.ActiveShippingRateComputationMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
				{
					shippingSettings2.ActiveShippingRateComputationMethodSystemNames.Remove(plugin.PluginDescriptor.SystemName);
					await settingService.SaveSettingAsync(shippingSettings2);
				}
				break;
			}
			case PluginType.PickupPointProvider:
			{
				ShippingSettings shippingSettings = engine.Resolve<ShippingSettings>();
				if (shippingSettings.ActivePickupPointProviderSystemNames.Contains(plugin.PluginDescriptor.SystemName))
				{
					shippingSettings.ActivePickupPointProviderSystemNames.Remove(plugin.PluginDescriptor.SystemName);
					await settingService.SaveSettingAsync(shippingSettings);
				}
				break;
			}
			case PluginType.WidgetPlugin:
			{
				WidgetSettings widgetSettings = engine.Resolve<WidgetSettings>();
				if (widgetSettings.ActiveWidgetSystemNames.Contains(plugin.PluginDescriptor.SystemName))
				{
					widgetSettings.ActiveWidgetSystemNames.Remove(plugin.PluginDescriptor.SystemName);
					await settingService.SaveSettingAsync(widgetSettings);
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			await engine.Resolve<ILogger>().ErrorAsync("Failed to enable " + plugin.PluginDescriptor.SystemName + ": " + ex.Message, ex);
		}
	}
}
