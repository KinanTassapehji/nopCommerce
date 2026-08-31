using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.PrevNextProduct;

public class PrevNextProductPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation prev/next product. Manage configuration", "ManageNopStationPrevNextProduct", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
