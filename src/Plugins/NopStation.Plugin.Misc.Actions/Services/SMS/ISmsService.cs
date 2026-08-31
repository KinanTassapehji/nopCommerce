using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Services.SMS;

public interface ISmsService
{
	Task<IList<ISmsPlugin>> GetActiveSmsPluginsAsync(Customer customer = null, int storeId = 0);

	Task<ISmsPlugin> GetSmsPluginBySystemNameAsync(string systemName, Customer customer = null, int storeId = 0);

	Task<SmsSendResult> SendSmsAsync(string phoneNumber, string body, Customer customer = null, int storeId = 0);

	Task<SmsSendResult> SendSmsAsync(string phoneNumber, string body, string pluginSystemName, Customer customer = null, int storeId = 0);
}
