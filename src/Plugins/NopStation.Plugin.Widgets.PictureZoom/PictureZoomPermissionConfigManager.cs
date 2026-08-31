using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.PictureZoom;

public class PictureZoomPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation picture zoom. Manage picture zoom", "ManageNopStationPictureZoom", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
