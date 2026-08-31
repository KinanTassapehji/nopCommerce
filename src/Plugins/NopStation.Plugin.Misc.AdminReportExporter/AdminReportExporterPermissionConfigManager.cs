using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.AdminReportExporter;

public class AdminReportExporterPermissionConfigManager : IPermissionConfigManager
{
	public IList<PermissionConfig> AllConfigs
	{
		get
		{
			List<PermissionConfig> list = new List<PermissionConfig>();
			list.Add(new PermissionConfig("NopStation Admin Report Exporter. Configuration", "ManageAdminReportExporterConfiguration", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			list.Add(new PermissionConfig("NopStation Admin Report Exporter. Manage AdminReportExporter", "ManageAdminReportExporter", "NopStation", NopCustomerDefaults.AdministratorsRoleName));
			return list;
		}
	}
}
