using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services.ReminderRules;

public class GenericReminderRule : BaseReminderRule
{
	public override string SystemName => "Generic";

	public GenericReminderRule(IRepository<Customer> customerRepository, IRepository<ReminderExcludedCustomer> reminderExcludedCustomerRepository)
		: base(customerRepository, reminderExcludedCustomerRepository)
	{
	}

	public override async Task<IList<Customer>> GetEligibleCustomersAsync(Reminder reminder, int dateGreaterThanMinutes, int dateLowerThanMinutes)
	{
		List<int> excludedCustomerIds = await GetExcludedCustomerIdsAsync(reminder.Id);
		IQueryable<Customer> source = _customerRepository.Table.Where((Customer c) => !c.Deleted && c.Active);
		if (excludedCustomerIds.Any())
		{
			source = source.Where((Customer c) => !excludedCustomerIds.Contains(c.Id));
		}
		if (reminder.StoreId > 0)
		{
			source = source.Where((Customer c) => c.RegisteredInStoreId == reminder.StoreId);
		}
		if (dateGreaterThanMinutes > 0 || dateLowerThanMinutes > 0)
		{
			(DateTime, DateTime) tuple = CalculateDateRange(dateGreaterThanMinutes, dateLowerThanMinutes);
			DateTime startDate = tuple.Item1;
			DateTime endDate = tuple.Item2;
			source = source.Where((Customer c) => c.CreatedOnUtc >= startDate && c.CreatedOnUtc <= endDate);
		}
		return await source.ToListAsync();
	}

	public override Task<DateTime?> GetConditionMetDateAsync(Customer customer)
	{
		return Task.FromResult((DateTime?)customer.CreatedOnUtc);
	}
}
