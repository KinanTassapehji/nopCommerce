using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.MegaMenu;

public class MegaMenuPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation mega menu. Manage mega-menu", "ManageNopStationMegaMenu", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
