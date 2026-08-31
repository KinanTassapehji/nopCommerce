using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services.ReminderRules;

public class AbandonedCartRule : BaseReminderRule
{
	private readonly IRepository<ShoppingCartItem> _shoppingCartRepository;

	private readonly IRepository<Order> _orderRepository;

	public override string SystemName => "AbandonedCart";

	public AbandonedCartRule(IRepository<Customer> customerRepository, IRepository<ReminderExcludedCustomer> reminderExcludedCustomerRepository, IRepository<ShoppingCartItem> shoppingCartRepository, IRepository<Order> orderRepository)
		: base(customerRepository, reminderExcludedCustomerRepository)
	{
		_shoppingCartRepository = shoppingCartRepository;
		_orderRepository = orderRepository;
	}

	public override async Task<IList<Customer>> GetEligibleCustomersAsync(Reminder reminder, int dateGreaterThanMinutes, int dateLowerThanMinutes)
	{
		(DateTime, DateTime) tuple = CalculateDateRange(dateGreaterThanMinutes, dateLowerThanMinutes);
		DateTime startDate = tuple.Item1;
		DateTime endDate = tuple.Item2;
		List<int> customersWithCarts = await (from sci in _shoppingCartRepository.Table
			where sci.ShoppingCartTypeId == 1 && sci.UpdatedOnUtc >= startDate && sci.UpdatedOnUtc <= endDate
			select sci.CustomerId).Distinct().ToListAsync();
		if (!customersWithCarts.Any())
		{
			return new List<Customer>();
		}
		List<int> second = await (from o in _orderRepository.Table
			where customersWithCarts.Contains(o.CustomerId) && o.CreatedOnUtc >= startDate
			select o.CustomerId).Distinct().ToListAsync();
		List<int> eligibleCustomerIds = customersWithCarts.Except(second).ToList();
		if (!eligibleCustomerIds.Any())
		{
			return new List<Customer>();
		}
		List<int> list = await GetExcludedCustomerIdsAsync(reminder.Id);
		if (list.Any())
		{
			eligibleCustomerIds = eligibleCustomerIds.Except(list).ToList();
		}
		if (!eligibleCustomerIds.Any())
		{
			return new List<Customer>();
		}
		IQueryable<Customer> source = _customerRepository.Table.Where((Customer c) => eligibleCustomerIds.Contains(c.Id) && !c.Deleted);
		if (reminder.StoreId > 0)
		{
			source = source.Where((Customer c) => c.RegisteredInStoreId == reminder.StoreId);
		}
		if (reminder.VendorId > 0)
		{
			source = source.Where((Customer c) => c.VendorId == reminder.VendorId);
		}
		return await source.ToListAsync();
	}

	public override async Task<DateTime?> GetConditionMetDateAsync(Customer customer)
	{
		return await (from sci in _shoppingCartRepository.Table
			where sci.CustomerId == customer.Id && sci.ShoppingCartTypeId == 1
			orderby sci.UpdatedOnUtc descending
			select sci.UpdatedOnUtc).FirstOrDefaultAsync();
	}
}
