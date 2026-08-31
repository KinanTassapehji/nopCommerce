using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.CancelOrder;

public class CancelOrderSettings : ISettings
{
	public string WidgetZone { get; set; }

	public List<int> CancellableOrderStatuses { get; set; }

	public List<int> CancellablePaymentStatuses { get; set; }

	public List<int> CancellableShippingStatuses { get; set; }

	public CancelOrderSettings()
	{
		CancellableOrderStatuses = new List<int>();
		CancellablePaymentStatuses = new List<int>();
		CancellableShippingStatuses = new List<int>();
	}
}
