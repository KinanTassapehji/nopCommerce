using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.ProductRibbon;

public class ProductRibbonPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation product ribbon. Manage product ribbon", "ManageNopStationProductRibbon", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
