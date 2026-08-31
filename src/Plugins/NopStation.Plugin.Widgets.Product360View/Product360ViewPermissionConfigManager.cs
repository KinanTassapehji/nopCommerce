using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.Product360View;

public class Product360ViewPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation Product 360 View. Manage Abandoned Carts", "ManageNopStationProduct360View", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
