using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services.ReminderRules;

public interface IReminderRuleImplementation
{
	string SystemName { get; }

	Task<IList<Customer>> GetEligibleCustomersAsync(Reminder reminder, int dateGreaterThanMinutes, int dateLowerThanMinutes);

	Task<DateTime?> GetConditionMetDateAsync(Customer customer);
}
