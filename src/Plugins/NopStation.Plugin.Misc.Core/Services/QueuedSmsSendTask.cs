using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Misc.Core.Domains.SMS;
using NopStation.Plugin.Misc.Core.Services.SMS;

namespace NopStation.Plugin.Misc.Core.Services;

public class QueuedSmsSendTask : IScheduleTask
{
	private readonly ILogger _logger;

	private readonly IQueuedSmsService _queuedSmsService;

	private readonly ISmsService _smsService;

	private readonly SmsSettings _smsSettings;

	private IDictionary<int, IDictionary<string, ISmsPlugin>> _smsPlugins = new Dictionary<int, IDictionary<string, ISmsPlugin>>();

	public QueuedSmsSendTask(ILogger logger, IQueuedSmsService queuedSmsService, ISmsService smsService, SmsSettings smsSettings)
	{
		_logger = logger;
		_queuedSmsService = queuedSmsService;
		_smsService = smsService;
		_smsSettings = smsSettings;
	}

	protected virtual string AppendError(string error, int sentTries, string message)
	{
		string value = (string.IsNullOrWhiteSpace(error) ? string.Empty : error);
		return $"{value}{sentTries}. {message}<br>";
	}

	protected virtual async Task<ISmsPlugin> GetMatchingPluginAsync(IDictionary<string, ISmsPlugin> pluginsLookup, string phoneNumber, string pluginSystemName = null)
	{
		if (!string.IsNullOrEmpty(pluginSystemName))
		{
			bool flag = pluginsLookup.TryGetValue(pluginSystemName, out var smsPlugin);
			if (flag)
			{
				flag = await smsPlugin.ValidatePhoneNumberAsync(phoneNumber);
			}
			if (flag)
			{
				return smsPlugin;
			}
		}
		foreach (KeyValuePair<string, ISmsPlugin> item in pluginsLookup)
		{
			ISmsPlugin smsPlugin = item.Value;
			if (await smsPlugin.ValidatePhoneNumberAsync(phoneNumber))
			{
				return smsPlugin;
			}
		}
		return null;
	}

	protected IDictionary<string, ISmsPlugin> FilterStorePlugins(IList<ISmsPlugin> plugins, int storeId)
	{
		if (_smsPlugins.TryGetValue(storeId, out var value))
		{
			return value;
		}
		value = new Dictionary<string, ISmsPlugin>();
		foreach (ISmsPlugin plugin in plugins)
		{
			if (!plugin.PluginDescriptor.LimitedToStores.Any() || plugin.PluginDescriptor.LimitedToStores.Contains(storeId))
			{
				value[plugin.PluginDescriptor.SystemName] = plugin;
			}
		}
		_smsPlugins[storeId] = value;
		return value;
	}

	public async Task ExecuteAsync()
	{
		try
		{
			IList<ISmsPlugin> plugins = await _smsService.GetActiveSmsPluginsAsync();
			int successCount = 0;
			int failureCount = 0;
			int skippedCount = 0;
			while (true)
			{
				IQueuedSmsService queuedSmsService = _queuedSmsService;
				int maxSendTries = _smsSettings.MaxSendTries;
				int maxMessagesPerBatch = _smsSettings.MaxMessagesPerBatch;
				IPagedList<QueuedSms> pagedList = await queuedSmsService.GetAllQueuedSmsAsync(loadOnlyItemsToBeSent: true, maxSendTries, null, null, null, 0, maxMessagesPerBatch);
				if (!pagedList.Any())
				{
					break;
				}
				foreach (QueuedSms queuedSms in pagedList)
				{
					try
					{
						ISmsPlugin smsPlugin = await GetMatchingPluginAsync(pluginSystemName: queuedSms.ProviderSystemName, pluginsLookup: FilterStorePlugins(plugins, queuedSms.StoreId), phoneNumber: queuedSms.PhoneNumber);
						if (smsPlugin == null)
						{
							queuedSms.SentTries++;
							queuedSms.Error = AppendError(queuedSms.Error, queuedSms.SentTries, "No provider available for this phone number");
							await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
							skippedCount++;
							continue;
						}
						string pluginSystemName = smsPlugin.PluginDescriptor.SystemName;
						SmsSendResult smsSendResult = await smsPlugin.SendSmsAsync(queuedSms.PhoneNumber, queuedSms.Body);
						if (!smsSendResult.Success)
						{
							queuedSms.SentTries++;
							queuedSms.Error = AppendError(queuedSms.Error, queuedSms.SentTries, smsSendResult.Exception?.Message ?? ("Failed to send SMS by provider (" + pluginSystemName + ")"));
							await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
							failureCount++;
							continue;
						}
						successCount++;
						queuedSms.SentOnUtc = DateTime.UtcNow;
						queuedSms.ExternalMessageId = smsSendResult.ExternalMessageId;
						queuedSms.SentTries++;
						queuedSms.Error = null;
						queuedSms.ProviderSystemName = pluginSystemName;
						await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
					}
					catch (Exception ex)
					{
						await _logger.ErrorAsync($"QueuedSmsSendTask: Exception while processing SMS {queuedSms.Id}", ex);
						queuedSms.SentTries++;
						queuedSms.Error = AppendError(queuedSms.Error, queuedSms.SentTries, "Exception: " + ex.Message);
						await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
						failureCount++;
					}
					if (_smsSettings.DelayBetweenMessagesMs > 0)
					{
						await Task.Delay(_smsSettings.DelayBetweenMessagesMs);
					}
				}
				await _logger.InformationAsync($"QueuedSmsSendTask: Completed. Success: {successCount}, Failed: {failureCount}, Skipped: {skippedCount}");
			}
		}
		catch (Exception exception)
		{
			await _logger.ErrorAsync("QueuedSmsSendTask: Fatal error during task execution", exception);
		}
	}
}
