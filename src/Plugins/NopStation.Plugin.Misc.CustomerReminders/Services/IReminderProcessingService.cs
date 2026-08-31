using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services;

public interface IReminderProcessingService
{
	Task ProcessRemindersAsync();

	Task ProcessSingleReminderAsync(Reminder reminder);

	Task<bool> ShouldSendReminderAsync(Customer customer, Reminder reminder, DateTime? conditionMetDate);

	Task<bool> SendReminderEmailAsync(Customer customer, Reminder reminder, DateTime? conditionMetDate);
}
