using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services.ReminderRules;

public class BirthdayRule : BaseReminderRule
{
	public override string SystemName => "Birthday";

	public BirthdayRule(IRepository<Customer> customerRepository, IRepository<ReminderExcludedCustomer> reminderExcludedCustomerRepository)
		: base(customerRepository, reminderExcludedCustomerRepository)
	{
	}

	public override async Task<IList<Customer>> GetEligibleCustomersAsync(Reminder reminder, int dateGreaterThanMinutes, int dateLowerThanMinutes)
	{
		(DateTime, DateTime) tuple = CalculateDateRange(dateGreaterThanMinutes, dateLowerThanMinutes);
		DateTime startDate = tuple.Item1;
		DateTime endDate = tuple.Item2;
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
		if (reminder.VendorId > 0)
		{
			source = source.Where((Customer c) => c.VendorId == reminder.VendorId);
		}
		List<Customer> obj = await source.ToListAsync();
		List<Customer> list = new List<Customer>();
		foreach (Customer item in obj)
		{
			DateTime? dateOfBirth = item.DateOfBirth;
			if (dateOfBirth.HasValue)
			{
				DateTime dateTime = new DateTime(DateTime.UtcNow.Year, dateOfBirth.Value.Month, dateOfBirth.Value.Day, 0, 0, 0, DateTimeKind.Utc);
				if (dateTime >= startDate.Date && dateTime <= endDate.Date)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public override Task<DateTime?> GetConditionMetDateAsync(Customer customer)
	{
		DateTime? dateOfBirth = customer.DateOfBirth;
		if (!dateOfBirth.HasValue)
		{
			return Task.FromResult<DateTime?>(null);
		}
		return Task.FromResult((DateTime?)new DateTime(DateTime.UtcNow.Year, dateOfBirth.Value.Month, dateOfBirth.Value.Day, 0, 0, 0, DateTimeKind.Utc));
	}
}
