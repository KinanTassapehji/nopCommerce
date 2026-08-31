using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Customers;

namespace NopStation.Plugin.Misc.Core.Services;

public class NopStationCustomerService : INopStationCustomerService
{
	private readonly IRepository<Customer> _customerRepository;

	private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;

	private readonly ICustomerService _customerService;

	public NopStationCustomerService(IRepository<Customer> customerRepository, IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository, ICustomerService customerService)
	{
		_customerRepository = customerRepository;
		_customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
		_customerService = customerService;
	}

	public Task<string> FormatCustomerNameAsync(Customer customer)
	{
		ArgumentNullException.ThrowIfNull(customer, "customer");
		string text = string.Empty;
		if (!string.IsNullOrEmpty(customer.FirstName))
		{
			text = customer.FirstName;
		}
		if (!string.IsNullOrEmpty(customer.LastName))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += " ";
			}
			text += customer.LastName;
		}
		if (!string.IsNullOrEmpty(customer.Email))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += " ~ ";
			}
			text += customer.Email;
		}
		return Task.FromResult(text);
	}

	public async Task<IPagedList<Customer>> GetCustomersAsync(string q = null, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		CustomerRole registeredRole = (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName)) ?? throw new NopException("'Registered' role could not be loaded");
		return await (from c in _customerRepository.Table
			join cr in _customerCustomerRoleMappingRepository.Table on c.Id equals cr.CustomerId
			where cr.CustomerRoleId == registeredRole.Id
			where !c.Deleted && (showHidden || c.Active) && (string.IsNullOrEmpty(q) || c.Email.Contains(q) || c.FirstName.Contains(q) || c.LastName.Contains(q) || string.Concat(c.FirstName + " ", c.LastName).Contains(q))
			select c).ToPagedListAsync(pageIndex, pageSize);
	}
}
