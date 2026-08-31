using System;
using System.Threading.Tasks;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Misc.CustomerReminders.Services;
using NopStation.Plugin.Misc.CustomerReminders.Settings;

namespace NopStation.Plugin.Misc.CustomerReminders.Tasks;

public class ReminderProcessingTask : IScheduleTask
{
	private readonly IReminderProcessingService _reminderProcessingService;

	private readonly INotificationService _notificationService;

	private readonly IWidgetPluginManager _widgetPluginManager;

	private readonly ISettingService _settingService;

	private readonly ILogger _logger;

	public ReminderProcessingTask(IReminderProcessingService reminderProcessingService, INotificationService notificationService, ISettingService settingService, IWidgetPluginManager widgetPluginManager, ILogger logger)
	{
		_reminderProcessingService = reminderProcessingService;
		_notificationService = notificationService;
		_widgetPluginManager = widgetPluginManager;
		_settingService = settingService;
		_logger = logger;
	}

	public async Task ExecuteAsync()
	{
		try
		{
			if (!(await _widgetPluginManager.IsPluginActiveAsync(CustomerRemindersDefaults.PluginSystemName)))
			{
				await _logger.InformationAsync("ReminderProcessingTask: Plugin is not active. Skipping execution.");
				return;
			}
			if (!(await _settingService.LoadSettingAsync<CustomerRemindersSettings>()).IsEnabled)
			{
				await _logger.InformationAsync("ReminderProcessingTask: Plugin is disabled. Skipping execution.");
				_notificationService.ErrorNotification("Customer Reminders plugin is disabled. Reminder processing task will not run.");
				return;
			}
			await _logger.InformationAsync("ReminderProcessingTask: Started");
			await _reminderProcessingService.ProcessRemindersAsync();
			await _logger.InformationAsync("ReminderProcessingTask: Completed successfully");
		}
		catch (Exception exception)
		{
			await _logger.ErrorAsync("ReminderProcessingTask: Error during execution", exception);
		}
	}
}
