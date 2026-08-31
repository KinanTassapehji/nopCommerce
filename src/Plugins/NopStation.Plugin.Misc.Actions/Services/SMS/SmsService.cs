using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Services.SMS;

public class SmsService : ISmsService
{
	private readonly ISmsPluginManager _smsPluginManager;

	public SmsService(ISmsPluginManager smsPluginManager)
	{
		_smsPluginManager = smsPluginManager;
	}

	protected virtual SmsSendResult CreateFailedResult(string message)
	{
		return new SmsSendResult
		{
			Success = false,
			Message = message
		};
	}

	public async Task<IList<ISmsPlugin>> GetActiveSmsPluginsAsync(Customer customer = null, int storeId = 0)
	{
		IList<ISmsPlugin> list = await _smsPluginManager.LoadSmsPluginsAsync(customer, "", storeId);
		List<ISmsPlugin> activePlugins = new List<ISmsPlugin>();
		foreach (ISmsPlugin plugin in list)
		{
			if (await plugin.IsActiveAsync())
			{
				activePlugins.Add(plugin);
			}
		}
		return activePlugins;
	}

	public async Task<ISmsPlugin> GetSmsPluginBySystemNameAsync(string systemName, Customer customer = null, int storeId = 0)
	{
		if (string.IsNullOrWhiteSpace(systemName))
		{
			return null;
		}
		ISmsPlugin plugin = (await _smsPluginManager.LoadSmsPluginsAsync(customer, systemName)).FirstOrDefault();
		bool flag = plugin != null;
		if (flag)
		{
			flag = await plugin.IsActiveAsync();
		}
		if (flag)
		{
			return plugin;
		}
		return null;
	}

	public async Task<SmsSendResult> SendSmsAsync(string phoneNumber, string body, Customer customer = null, int storeId = 0)
	{
		foreach (ISmsPlugin plugin in await GetActiveSmsPluginsAsync(customer, storeId))
		{
			if (await plugin.ValidatePhoneNumberAsync(phoneNumber))
			{
				return await plugin.SendSmsAsync(phoneNumber, body);
			}
		}
		return CreateFailedResult("No compatible SMS plugin found");
	}

	public async Task<SmsSendResult> SendSmsAsync(string phoneNumber, string body, string pluginSystemName, Customer customer = null, int storeId = 0)
	{
		ISmsPlugin plugin = await GetSmsPluginBySystemNameAsync(pluginSystemName, customer, storeId);
		bool flag = plugin == null;
		if (!flag)
		{
			flag = !(await plugin.ValidatePhoneNumberAsync(phoneNumber));
		}
		if (flag)
		{
			return CreateFailedResult("Plugin '" + pluginSystemName + "' not found or disabled");
		}
		return await plugin.SendSmsAsync(phoneNumber, body);
	}
}
