using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Plugins;

namespace NopStation.Plugin.Misc.Core.Services.SMS;

public class SmsPluginManager : PluginManager<ISmsPlugin>, ISmsPluginManager
{
	public SmsPluginManager(ICustomerService customerService, IPluginService pluginService)
		: base(customerService, pluginService)
	{
	}

	public virtual async Task<IList<ISmsPlugin>> LoadSmsPluginsAsync(Customer customer = null, string pluginSystemName = "", int storeId = 0)
	{
		return (await LoadAllPluginsAsync(customer, storeId)).Where((ISmsPlugin plugin) => string.IsNullOrWhiteSpace(pluginSystemName) || plugin.PluginDescriptor.SystemName.Equals(pluginSystemName)).ToList();
	}
}
