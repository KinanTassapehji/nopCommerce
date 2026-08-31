using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services.ReminderRules;

public abstract class BaseReminderRule : IReminderRuleImplementation
{
	protected readonly IRepository<Customer> _customerRepository;

	protected readonly IRepository<ReminderExcludedCustomer> _reminderExcludedCustomerRepository;

	public abstract string SystemName { get; }

	protected BaseReminderRule(IRepository<Customer> customerRepository, IRepository<ReminderExcludedCustomer> reminderExcludedCustomerRepository)
	{
		_customerRepository = customerRepository;
		_reminderExcludedCustomerRepository = reminderExcludedCustomerRepository;
	}

	public abstract Task<IList<Customer>> GetEligibleCustomersAsync(Reminder reminder, int dateGreaterThanMinutes, int dateLowerThanMinutes);

	public abstract Task<DateTime?> GetConditionMetDateAsync(Customer customer);

	protected (DateTime startDate, DateTime endDate) CalculateDateRange(int dateGreaterThanMinutes, int dateLowerThanMinutes)
	{
		DateTime utcNow = DateTime.UtcNow;
		DateTime item = utcNow.AddMinutes(-dateLowerThanMinutes);
		DateTime item2 = utcNow.AddMinutes(-dateGreaterThanMinutes);
		return (startDate: item, endDate: item2);
	}

	protected async Task<List<int>> GetExcludedCustomerIdsAsync(int reminderId)
	{
		return await (from rec in _reminderExcludedCustomerRepository.Table
			where rec.ReminderId == reminderId
			select rec.CustomerId).ToListAsync();
	}
}
