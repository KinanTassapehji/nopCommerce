using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.CancelOrder.Components;

namespace NopStation.Plugin.Widgets.CancelOrder;

public class CancelOrderPlugin : BasePlugin, IWidgetPlugin, IPlugin, INopStationPlugin
{
	private readonly IWebHelper _webHelper;

	private readonly ISettingService _settingService;

	private readonly ILocalizationService _localizationService;

	private readonly CancelOrderSettings _cancelOrderSettings;

	private readonly IPermissionService _permissionService;

	public bool HideInWidgetList => false;

	public CancelOrderPlugin(IWebHelper webHelper, ISettingService settingService, ILocalizationService localizationService, CancelOrderSettings cancelOrderSettings, IPermissionService permissionService)
	{
		_webHelper = webHelper;
		_settingService = settingService;
		_localizationService = localizationService;
		_cancelOrderSettings = cancelOrderSettings;
		_permissionService = permissionService;
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/CancelOrder/Configure";
	}

	public override async Task InstallAsync()
	{
		CancelOrderSettings settings = new CancelOrderSettings
		{
			WidgetZone = PublicWidgetZones.OrderDetailsPageBottom,
			CancellableOrderStatuses = new List<int> { 10, 20 },
			CancellablePaymentStatuses = new List<int> { 10 },
			CancellableShippingStatuses = new List<int> { 20 }
		};
		await _settingService.SaveSettingAsync(settings);
		await this.InstallPluginAsync();
		await base.InstallAsync();
	}

	public override async Task UninstallAsync()
	{
		await this.UninstallPluginAsync();
		await base.UninstallAsync();
	}

	public Type GetWidgetViewComponent(string widgetZone)
	{
		return typeof(CancelOrderViewComponent);
	}

	public Task<IList<string>> GetWidgetZonesAsync()
	{
		string item = (string.IsNullOrWhiteSpace(_cancelOrderSettings.WidgetZone) ? PublicWidgetZones.OrderDetailsPageBottom : _cancelOrderSettings.WidgetZone);
		return Task.FromResult((IList<string>)new List<string> { item });
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["Admin.NopStation.CancelOrder.Menu.CancelOrder"] = "Cancel order",
			["Admin.NopStation.CancelOrder.Menu.Configuration"] = "Configuration",
			["NopStation.CancelOrder.OrderCancelledByCustomer"] = "Order cancelled by customer",
			["NopStation.CancelOrder.InvalidRequest"] = "Invalid order cancel request.",
			["NopStation.CancelOrder.OrderNotFound"] = "Order not found!",
			["NopStation.CancelOrder.Button"] = "Cancel order",
			["NopStation.CancelOrder.Confirm"] = "Are you sure to cancel order?",
			["Admin.NopStation.CancelOrder.Configuration"] = "Cancel order settings",
			["Admin.NopStation.CancelOrder.Configuration.Fields.WidgetZone"] = "Widget zone",
			["Admin.NopStation.CancelOrder.Configuration.Fields.WidgetZone.Hint"] = "Specify widget zone where cancel button will be displayed in order details page.",
			["Admin.NopStation.CancelOrder.Configuration.Fields.CancellableOrderStatuses"] = "Cancellable order statuses",
			["Admin.NopStation.CancelOrder.Configuration.Fields.CancellableOrderStatuses.Hint"] = "Specify cancellable order statuses.",
			["Admin.NopStation.CancelOrder.Configuration.Fields.CancellablePaymentStatuses"] = "Cancellable payment statuses",
			["Admin.NopStation.CancelOrder.Configuration.Fields.CancellablePaymentStatuses.Hint"] = "Specify cancellable payment statuses.",
			["Admin.NopStation.CancelOrder.Configuration.Fields.CancellableShippingStatuses"] = "Cancellable shipping statuses",
			["Admin.NopStation.CancelOrder.Configuration.Fields.CancellableShippingStatuses.Hint"] = "Specify cancellable shipping statuses."
		};
	}
}
