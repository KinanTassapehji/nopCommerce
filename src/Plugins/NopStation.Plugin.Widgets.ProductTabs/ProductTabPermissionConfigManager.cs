using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.ProductTabs;

public class ProductTabPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("Admin area. Manage NopStation product tab", "ManageNopStationProductTab", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
