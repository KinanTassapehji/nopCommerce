using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Services.SMS;

public class WorkflowSmsService : IWorkflowSmsService
{
	private readonly ICustomerService _customerService;

	private readonly ILanguageService _languageService;

	private readonly ILocalizationService _localizationService;

	private readonly ISmsTemplateService _smsTemplateService;

	private readonly ISmsTokenProvider _smsTokenProvider;

	private readonly IQueuedSmsService _queuedSmsService;

	private readonly IStoreContext _storeContext;

	private readonly IStoreService _storeService;

	private readonly ITokenizer _tokenizer;

	private readonly IOrderService _orderService;

	private readonly IForumService _forumService;

	private readonly IRepository<Customer> _customerRepository;

	private readonly IGenericAttributeService _genericAttributeService;

	private readonly IAclService _aclService;

	private readonly IAddressService _addressService;

	public WorkflowSmsService(ICustomerService customerService, ILanguageService languageService, ILocalizationService localizationService, ISmsTemplateService smsTemplateService, ISmsTokenProvider smsTokenProvider, IQueuedSmsService queuedSmsService, IStoreContext storeContext, IStoreService storeService, ITokenizer tokenizer, IOrderService orderService, IForumService forumService, IRepository<Customer> customerRepository, IGenericAttributeService genericAttributeService, IAclService aclService, IAddressService addressService)
	{
		_customerService = customerService;
		_languageService = languageService;
		_localizationService = localizationService;
		_smsTemplateService = smsTemplateService;
		_smsTokenProvider = smsTokenProvider;
		_queuedSmsService = queuedSmsService;
		_storeContext = storeContext;
		_storeService = storeService;
		_tokenizer = tokenizer;
		_orderService = orderService;
		_forumService = forumService;
		_customerRepository = customerRepository;
		_genericAttributeService = genericAttributeService;
		_aclService = aclService;
		_addressService = addressService;
	}

	protected virtual async Task<IList<SmsTemplate>> GetActiveSmsTemplatesAsync(Customer customer, string smsTemplateName, int storeId)
	{
		IList<SmsTemplate> list = await _smsTemplateService.GetSmsTemplatesByNameAsync(smsTemplateName, storeId);
		if (list == null || !list.Any())
		{
			return new List<SmsTemplate>();
		}
		list = list.Where((SmsTemplate smsTemplate) => smsTemplate.Active).ToList();
		if (customer != null)
		{
			list = await list.WhereAwait(async (SmsTemplate x) => await _aclService.AuthorizeAsync(x, customer)).ToListAsync();
		}
		return list;
	}

	protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
	{
		Language language = await _languageService.GetLanguageByIdAsync(languageId);
		if (language == null || !language.Published)
		{
			language = (await _languageService.GetAllLanguagesAsync(showHidden: false, storeId)).FirstOrDefault();
		}
		if (language == null || !language.Published)
		{
			language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
		}
		if (language == null)
		{
			throw new Exception("No active language could be loaded");
		}
		return language.Id;
	}

	protected async Task<string> GetVendorPhonenumberAsync(Vendor vendor, Customer customer, Store store)
	{
		string text = customer.Phone;
		if (string.IsNullOrWhiteSpace(text))
		{
			text = (await _addressService.GetAddressByIdAsync(vendor.AddressId))?.PhoneNumber;
		}
		return text;
	}

	protected string GetCustomerPhonenumber(Customer customer, string phoneNumber, Store store)
	{
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			phoneNumber = customer.Phone;
		}
		return phoneNumber;
	}

	public virtual async Task<IList<int>> SendCustomerRegisteredNotificationMessageAsync(Customer customer, int languageId)
	{
		ArgumentNullException.ThrowIfNull(customer, "customer");
		Store store = await _storeContext.GetCurrentStoreAsync();
		languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "NewCustomer.Notification", store.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		ICustomerService customerService = _customerService;
		CustomerRole customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
		int[] customerRoleIds = new int[1] { customerRole.Id };
		IPagedList<Customer> customers = await customerService.GetAllCustomersAsync(null, null, null, null, 0, 0, customerRoleIds);
		if (customers == null || !customers.Any())
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		List<int> ids = new List<int>();
		List<string> numbers = new List<string>();
		foreach (Customer admin in customers)
		{
			string phoneNumber = GetCustomerPhonenumber(admin, "", store);
			if (!string.IsNullOrWhiteSpace(phoneNumber) && !numbers.Contains(phoneNumber))
			{
				IAsyncEnumerable<SmsTemplate> source = notificationTemplates.WhereAwait(async (SmsTemplate x) => await _aclService.AuthorizeAsync(x, admin));
				List<int> list = ids;
				list.AddRange(await source.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
				{
					List<Token> tokens = new List<Token>(commonTokens);
					await _smsTokenProvider.AddStoreTokensAsync(tokens, store);
					return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, admin);
				}).ToListAsync());
			}
		}
		return ids;
	}

	public virtual async Task<IList<int>> SendCustomerWelcomeMessageAsync(Customer customer, int languageId)
	{
		ArgumentNullException.ThrowIfNull(customer, "customer");
		Store store = await _storeContext.GetCurrentStoreAsync();
		languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "Customer.WelcomeMessage", store.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		string phoneNumber = GetCustomerPhonenumber(customer, "", store);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendCustomerEmailValidationMessageAsync(Customer customer, int languageId)
	{
		ArgumentNullException.ThrowIfNull(customer, "customer");
		Store store = await _storeContext.GetCurrentStoreAsync();
		languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "Customer.EmailValidationMessage", store.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		string phoneNumber = GetCustomerPhonenumber(customer, "", store);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendOrderPlacedVendorNotificationAsync(Order order, Vendor vendor, int languageId)
	{
		ArgumentNullException.ThrowIfNull(order, "order");
		ArgumentNullException.ThrowIfNull(vendor, "vendor");
		IQueryable<Customer> customers = _customerRepository.Table.Where((Customer x) => x.VendorId == vendor.Id);
		if (customers == null || !customers.Any())
		{
			return new List<int>();
		}
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(null, "OrderPlaced.VendorNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId, vendor.Id);
		ISmsTokenProvider smsTokenProvider = _smsTokenProvider;
		IList<Token> commonTokens2 = commonTokens;
		await smsTokenProvider.AddCustomerTokensAsync(commonTokens2, await _customerService.GetCustomerByIdAsync(order.CustomerId));
		List<int> ids = new List<int>();
		List<string> numbers = new List<string>();
		foreach (Customer customer in customers)
		{
			string vendorPhoneNumber = await GetVendorPhonenumberAsync(vendor, customer, store2);
			if (string.IsNullOrWhiteSpace(vendorPhoneNumber) || numbers.Contains(vendorPhoneNumber))
			{
				return new List<int>();
			}
			numbers.Add(vendorPhoneNumber);
			IAsyncEnumerable<SmsTemplate> source = notificationTemplates.WhereAwait(async (SmsTemplate x) => await _aclService.AuthorizeAsync(x, customer));
			List<int> list = ids;
			list.AddRange(await source.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
			{
				List<Token> tokens = new List<Token>(commonTokens);
				await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
				return await SendSmsAsync(vendorPhoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
			}).ToListAsync());
		}
		return ids;
	}

	public virtual async Task<IList<int>> SendOrderPlacedAdminNotificationAsync(Order order, int languageId)
	{
		ArgumentNullException.ThrowIfNull(order, "order");
		ICustomerService customerService = _customerService;
		CustomerRole customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
		int[] customerRoleIds = new int[1] { customerRole.Id };
		IPagedList<Customer> customers = await customerService.GetAllCustomersAsync(null, null, null, null, 0, 0, customerRoleIds);
		if (customers == null || !customers.Any())
		{
			return new List<int>();
		}
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(null, "OrderPlaced.AdminNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
		ISmsTokenProvider smsTokenProvider = _smsTokenProvider;
		IList<Token> commonTokens2 = commonTokens;
		await smsTokenProvider.AddCustomerTokensAsync(commonTokens2, await _customerService.GetCustomerByIdAsync(order.CustomerId));
		List<int> ids = new List<int>();
		List<string> numbers = new List<string>();
		foreach (Customer customer in customers)
		{
			string phoneNumber = GetCustomerPhonenumber(customer, "", store2);
			if (!string.IsNullOrWhiteSpace(phoneNumber) && !numbers.Contains(phoneNumber))
			{
				IAsyncEnumerable<SmsTemplate> source = notificationTemplates.WhereAwait(async (SmsTemplate x) => await _aclService.AuthorizeAsync(x, customer));
				List<int> list = ids;
				list.AddRange(await source.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
				{
					List<Token> tokens = new List<Token>(commonTokens);
					await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
					return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
				}).ToListAsync());
			}
		}
		return ids;
	}

	public virtual async Task<IList<int>> SendOrderPaidAdminNotificationAsync(Order order, int languageId)
	{
		ArgumentNullException.ThrowIfNull(order, "order");
		ICustomerService customerService = _customerService;
		CustomerRole customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
		int[] customerRoleIds = new int[1] { customerRole.Id };
		IPagedList<Customer> customers = await customerService.GetAllCustomersAsync(null, null, null, null, 0, 0, customerRoleIds);
		if (customers == null || !customers.Any())
		{
			return new List<int>();
		}
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(null, "OrderPaid.AdminNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
		ISmsTokenProvider smsTokenProvider = _smsTokenProvider;
		IList<Token> commonTokens2 = commonTokens;
		await smsTokenProvider.AddCustomerTokensAsync(commonTokens2, await _customerService.GetCustomerByIdAsync(order.CustomerId));
		List<int> ids = new List<int>();
		List<string> numbers = new List<string>();
		foreach (Customer customer in customers)
		{
			string phoneNumber = GetCustomerPhonenumber(customer, "", store2);
			if (!string.IsNullOrWhiteSpace(phoneNumber) && !numbers.Contains(phoneNumber))
			{
				IAsyncEnumerable<SmsTemplate> source = notificationTemplates.WhereAwait(async (SmsTemplate x) => await _aclService.AuthorizeAsync(x, customer));
				List<int> list = ids;
				list.AddRange(await source.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
				{
					List<Token> tokens = new List<Token>(commonTokens);
					await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
					return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
				}).ToListAsync());
			}
		}
		return ids;
	}

	public virtual async Task<IList<int>> SendOrderPaidCustomerNotificationAsync(Order order, int languageId)
	{
		ArgumentNullException.ThrowIfNull(order, "order");
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		Customer customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "OrderPaid.CustomerNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		Address address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
		string phoneNumber = GetCustomerPhonenumber(customer, address?.PhoneNumber, store2);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendOrderPaidVendorNotificationAsync(Order order, Vendor vendor, int languageId)
	{
		ArgumentNullException.ThrowIfNull(order, "order");
		ArgumentNullException.ThrowIfNull(vendor, "vendor");
		IQueryable<Customer> customers = _customerRepository.Table.Where((Customer x) => x.VendorId == vendor.Id);
		if (customers == null || !customers.Any())
		{
			return new List<int>();
		}
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(null, "OrderPaid.VendorNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId, vendor.Id);
		ISmsTokenProvider smsTokenProvider = _smsTokenProvider;
		IList<Token> commonTokens2 = commonTokens;
		await smsTokenProvider.AddCustomerTokensAsync(commonTokens2, await _customerService.GetCustomerByIdAsync(order.CustomerId));
		List<int> ids = new List<int>();
		List<string> numbers = new List<string>();
		foreach (Customer customer in customers)
		{
			string vendorPhoneNumber = await GetVendorPhonenumberAsync(vendor, customer, store2);
			if (string.IsNullOrWhiteSpace(vendorPhoneNumber) || numbers.Contains(vendorPhoneNumber))
			{
				return new List<int>();
			}
			numbers.Add(vendorPhoneNumber);
			IAsyncEnumerable<SmsTemplate> source = notificationTemplates.WhereAwait(async (SmsTemplate x) => await _aclService.AuthorizeAsync(x, customer));
			List<int> list = ids;
			list.AddRange(await source.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
			{
				List<Token> tokens = new List<Token>(commonTokens);
				await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
				return await SendSmsAsync(vendorPhoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
			}).ToListAsync());
		}
		return ids;
	}

	public virtual async Task<IList<int>> SendOrderPlacedCustomerNotificationAsync(Order order, int languageId)
	{
		ArgumentNullException.ThrowIfNull(order, "order");
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		Customer customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "OrderPlaced.CustomerNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		Address address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
		string phoneNumber = GetCustomerPhonenumber(customer, address?.PhoneNumber, store2);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendShipmentSentCustomerNotificationAsync(Shipment shipment, int languageId)
	{
		ArgumentNullException.ThrowIfNull(shipment, "shipment");
		Order order = (await _orderService.GetOrderByIdAsync(shipment.OrderId)) ?? throw new Exception("Order cannot be loaded");
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		Customer customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "ShipmentSent.CustomerNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		Address address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
		string phoneNumber = GetCustomerPhonenumber(customer, address?.PhoneNumber, store2);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddShipmentTokensAsync(commonTokens, shipment, languageId);
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendShipmentDeliveredCustomerNotificationAsync(Shipment shipment, int languageId)
	{
		ArgumentNullException.ThrowIfNull(shipment, "shipment");
		Order order = (await _orderService.GetOrderByIdAsync(shipment.OrderId)) ?? throw new Exception("Order cannot be loaded");
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		Customer customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "ShipmentDelivered.CustomerNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		Address address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
		string phoneNumber = GetCustomerPhonenumber(customer, address?.PhoneNumber, store2);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddShipmentTokensAsync(commonTokens, shipment, languageId);
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendShipmentDeliveredCustomerOTPNotificationAsync(Shipment shipment, string otp, int languageId)
	{
		ArgumentNullException.ThrowIfNull(shipment, "shipment");
		Order order = (await _orderService.GetOrderByIdAsync(shipment.OrderId)) ?? throw new Exception("Order cannot be loaded");
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		Customer customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "ShipmentDelivered.CustomerOTPNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		Address address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
		string phoneNumber = GetCustomerPhonenumber(customer, address?.PhoneNumber, store2);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddShipmentTokensAsync(commonTokens, shipment, languageId);
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		_smsTokenProvider.AddOTPTokens(commonTokens, otp);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendOrderCompletedCustomerNotificationAsync(Order order, int languageId)
	{
		ArgumentNullException.ThrowIfNull(order, "order");
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		Customer customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "OrderCompleted.CustomerNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		Address address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
		string phoneNumber = GetCustomerPhonenumber(customer, address?.PhoneNumber, store2);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendOrderCancelledCustomerNotificationAsync(Order order, int languageId)
	{
		ArgumentNullException.ThrowIfNull(order, "order");
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		Customer customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "OrderCancelled.CustomerNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		Address address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
		string phoneNumber = GetCustomerPhonenumber(customer, address?.PhoneNumber, store2);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendOrderRefundedAdminNotificationAsync(Order order, decimal refundedAmount, int languageId)
	{
		ArgumentNullException.ThrowIfNull(order, "order");
		ICustomerService customerService = _customerService;
		CustomerRole customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
		int[] customerRoleIds = new int[1] { customerRole.Id };
		IPagedList<Customer> customers = await customerService.GetAllCustomersAsync(null, null, null, null, 0, 0, customerRoleIds);
		if (customers == null || !customers.Any())
		{
			return new List<int>();
		}
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(null, "OrderRefunded.AdminNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
		await _smsTokenProvider.AddOrderRefundedTokensAsync(commonTokens, order, refundedAmount);
		ISmsTokenProvider smsTokenProvider = _smsTokenProvider;
		IList<Token> commonTokens2 = commonTokens;
		await smsTokenProvider.AddCustomerTokensAsync(commonTokens2, await _customerService.GetCustomerByIdAsync(order.CustomerId));
		List<int> ids = new List<int>();
		List<string> numbers = new List<string>();
		foreach (Customer customer in customers)
		{
			Address address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
			string phoneNumber = GetCustomerPhonenumber(customer, address?.PhoneNumber, store2);
			if (!string.IsNullOrWhiteSpace(phoneNumber) && !numbers.Contains(phoneNumber))
			{
				numbers.Add(phoneNumber);
				IAsyncEnumerable<SmsTemplate> source = notificationTemplates.WhereAwait(async (SmsTemplate x) => await _aclService.AuthorizeAsync(x, customer));
				List<int> list = ids;
				list.AddRange(await source.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
				{
					List<Token> tokens = new List<Token>(commonTokens);
					await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
					return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
				}).ToListAsync());
			}
		}
		return ids;
	}

	public virtual async Task<IList<int>> SendOrderRefundedCustomerNotificationAsync(Order order, decimal refundedAmount, int languageId)
	{
		ArgumentNullException.ThrowIfNull(order, "order");
		Store store = await _storeService.GetStoreByIdAsync(order.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		languageId = await EnsureLanguageIsActiveAsync(languageId, store2.Id);
		Customer customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "OrderRefunded.CustomerNotification", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		Address address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
		string phoneNumber = GetCustomerPhonenumber(customer, address?.PhoneNumber, store2);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
		await _smsTokenProvider.AddOrderRefundedTokensAsync(commonTokens, order, refundedAmount);
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendNewForumTopicMessageAsync(Customer customer, ForumTopic forumTopic, Forum forum, int languageId)
	{
		ArgumentNullException.ThrowIfNull(customer, "customer");
		Store store = await _storeContext.GetCurrentStoreAsync();
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "Forums.NewForumTopic", store.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		string phoneNumber = GetCustomerPhonenumber(customer, "", store);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		Forum forums = (await _forumService.GetForumByIdAsync(forumTopic.ForumId)) ?? throw new ArgumentException("forum cannot be loaded");
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddForumTopicTokensAsync(commonTokens, forumTopic);
		await _smsTokenProvider.AddForumTokensAsync(commonTokens, forums);
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendNewForumPostMessageAsync(Customer customer, ForumPost forumPost, ForumTopic forumTopic, Forum forum, int friendlyForumTopicPageIndex, int languageId)
	{
		ArgumentNullException.ThrowIfNull(customer, "customer");
		Store store = await _storeContext.GetCurrentStoreAsync();
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "Forums.NewForumPost", store.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		string phoneNumber = GetCustomerPhonenumber(customer, "", store);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		ForumTopic forumTopics = (await _forumService.GetTopicByIdAsync(forumPost.TopicId)) ?? throw new ArgumentException("forum topic cannot be loaded");
		Forum forums = (await _forumService.GetForumByIdAsync(forumTopics.ForumId)) ?? throw new ArgumentException("forum cannot be loaded");
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddForumPostTokensAsync(commonTokens, forumPost);
		await _smsTokenProvider.AddForumTopicTokensAsync(commonTokens, forumTopics, friendlyForumTopicPageIndex, forumPost.Id);
		await _smsTokenProvider.AddForumTokensAsync(commonTokens, forums);
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<IList<int>> SendPrivateMessageNotificationAsync(PrivateMessage privateMessage, int languageId)
	{
		ArgumentNullException.ThrowIfNull(privateMessage, "privateMessage");
		Store store = await _storeService.GetStoreByIdAsync(privateMessage.StoreId);
		if (store == null)
		{
			store = await _storeContext.GetCurrentStoreAsync();
		}
		Store store2 = store;
		Customer customer = await _customerService.GetCustomerByIdAsync(privateMessage.ToCustomerId);
		IList<SmsTemplate> notificationTemplates = await GetActiveSmsTemplatesAsync(customer, "Customer.NewPM", store2.Id);
		if (!notificationTemplates.Any())
		{
			return new List<int>();
		}
		string phoneNumber = GetCustomerPhonenumber(customer, "", store2);
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return new List<int>();
		}
		List<Token> commonTokens = new List<Token>();
		await _smsTokenProvider.AddPrivateMessageTokensAsync(commonTokens, privateMessage);
		await _smsTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
		return await notificationTemplates.SelectAwait<SmsTemplate, int>(async delegate(SmsTemplate smsTemplate)
		{
			List<Token> tokens = new List<Token>(commonTokens);
			await _smsTokenProvider.AddStoreTokensAsync(tokens, store2);
			return await SendSmsAsync(phoneNumber, smsTemplate, languageId, tokens, store2.Id, customer);
		}).ToListAsync();
	}

	public virtual async Task<int> SendSmsAsync(string phoneNumber, SmsTemplate smsTemplate, int languageId, IEnumerable<Token> tokens, int storeId, Customer customer)
	{
		ArgumentNullException.ThrowIfNull(smsTemplate, "smsTemplate");
		await _languageService.GetLanguageByIdAsync(languageId);
		string template = await _localizationService.GetLocalizedAsync(smsTemplate, (SmsTemplate mt) => mt.Body, languageId);
		string body = _tokenizer.Replace(template, tokens, htmlEncode: true);
		return await SendSmsAsync(phoneNumber, body, storeId, customer, smsTemplate.ProviderSystemName);
	}

	public async Task<int> SendSmsAsync(string phoneNumber, string body, int storeId, Customer customer, string providerSystemName = null)
	{
		if (string.IsNullOrWhiteSpace(phoneNumber))
		{
			return 0;
		}
		if (customer == null)
		{
			return 0;
		}
		QueuedSms queuedSms = new QueuedSms
		{
			Body = body,
			CreatedOnUtc = DateTime.UtcNow,
			CustomerId = customer.Id,
			StoreId = storeId,
			PhoneNumber = phoneNumber,
			ProviderSystemName = (providerSystemName ?? string.Empty),
			SentTries = 0
		};
		await _queuedSmsService.InsertQueuedSmsAsync(queuedSms);
		return queuedSms.Id;
	}
}
