using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.AnywhereSlider;

public class SliderPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation anywhere slider. Manage slider", "ManageNopStationSliders", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
