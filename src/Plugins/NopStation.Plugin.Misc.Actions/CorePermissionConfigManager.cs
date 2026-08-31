using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.Core;

public class CorePermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation core. Manage license", "ManageNopStationCoreLicense", NopStationCoreDefaults.CategoryName, NopCustomerDefaults.AdministratorsRoleName));
			list.Add(new PermissionConfig("NopStation core. Manage configuration", "ManageNopStationCoreConfiguration", NopStationCoreDefaults.CategoryName, NopCustomerDefaults.AdministratorsRoleName));
			list.Add(new PermissionConfig("NopStation core. Manage NopStation features", "ManageNopStationFeatures", NopStationCoreDefaults.CategoryName, NopCustomerDefaults.AdministratorsRoleName));
			list.Add(new PermissionConfig("NopStation core. Show Documentations", "ShowNopStationDocumentations", NopStationCoreDefaults.CategoryName, NopCustomerDefaults.AdministratorsRoleName));
			list.Add(new PermissionConfig("NopStation core. Manage SMS templates", "ManageNopStationSmsTemplates", NopStationCoreDefaults.CategoryName, NopCustomerDefaults.AdministratorsRoleName));
			list.Add(new PermissionConfig("NopStation core. Manage SMS queue", "ManageNopStationSmsQueue", NopStationCoreDefaults.CategoryName, NopCustomerDefaults.AdministratorsRoleName));
			list.Add(new PermissionConfig("NopStation core. Manage SMS configuration", "ManageNopStationSmsConfiguration", NopStationCoreDefaults.CategoryName, NopCustomerDefaults.AdministratorsRoleName));
			list.Add(new PermissionConfig("NopStation core. Manage SMS providers", "ManageNopStationSmsProviders", NopStationCoreDefaults.CategoryName, NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
