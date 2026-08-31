using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.QuickView;

public class QuickViewPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation quick view. Manage quick view", "ManageNopStationQuickView", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
