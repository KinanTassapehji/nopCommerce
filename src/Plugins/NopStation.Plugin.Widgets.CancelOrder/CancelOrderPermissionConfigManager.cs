using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.CancelOrder;

public class CancelOrderPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation Cancel Order. Manage Cancel Order", "ManageNopStationCancelOrder", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
