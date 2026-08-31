using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services.ReminderRules;

public class UnpaidOrdersRule : BaseReminderRule
{
	private readonly IRepository<Order> _orderRepository;

	public override string SystemName => "UnpaidOrders";

	public UnpaidOrdersRule(IRepository<Customer> customerRepository, IRepository<ReminderExcludedCustomer> reminderExcludedCustomerRepository, IRepository<Order> orderRepository)
		: base(customerRepository, reminderExcludedCustomerRepository)
	{
		_orderRepository = orderRepository;
	}

	public override async Task<IList<Customer>> GetEligibleCustomersAsync(Reminder reminder, int dateGreaterThanMinutes, int dateLowerThanMinutes)
	{
		(DateTime, DateTime) tuple = CalculateDateRange(dateGreaterThanMinutes, dateLowerThanMinutes);
		DateTime startDate = tuple.Item1;
		DateTime endDate = tuple.Item2;
		IQueryable<Order> source = _orderRepository.Table.Where((Order o) => o.PaymentStatusId == 10 && !o.Deleted && o.CreatedOnUtc >= startDate && o.CreatedOnUtc <= endDate);
		if (reminder.StoreId > 0)
		{
			source = source.Where((Order o) => o.StoreId == reminder.StoreId);
		}
		List<int> customerIds = await source.Select((Order o) => o.CustomerId).Distinct().ToListAsync();
		if (!customerIds.Any())
		{
			return new List<Customer>();
		}
		List<int> list = await GetExcludedCustomerIdsAsync(reminder.Id);
		if (list.Any())
		{
			customerIds = customerIds.Except(list).ToList();
		}
		if (!customerIds.Any())
		{
			return new List<Customer>();
		}
		if (reminder.VendorId > 0)
		{
			List<int> second = await (from c in _customerRepository.Table
				where c.VendorId == reminder.VendorId
				select c.Id).ToListAsync();
			customerIds = customerIds.Intersect(second).ToList();
		}
		if (!customerIds.Any())
		{
			return new List<Customer>();
		}
		return await _customerRepository.Table.Where((Customer c) => customerIds.Contains(c.Id) && !c.Deleted).ToListAsync();
	}

	public override async Task<DateTime?> GetConditionMetDateAsync(Customer customer)
	{
		return await (from o in _orderRepository.Table
			where o.CustomerId == customer.Id && o.PaymentStatusId == 10 && !o.Deleted
			orderby o.CreatedOnUtc descending
			select o.CreatedOnUtc).FirstOrDefaultAsync();
	}
}
