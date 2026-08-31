using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.OCarousels;

public class OCarouselPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation OCarousel. Manage carousels", "ManageNopStationOCarousels", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
