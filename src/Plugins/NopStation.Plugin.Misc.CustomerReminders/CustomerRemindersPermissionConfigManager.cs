using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.CustomerReminders;

public class CustomerRemindersPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation Customer Reminders. Manage permission", "ManageCustomerReminders", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
