using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Domains.Enums;
using NopStation.Plugin.Misc.CustomerReminders.Helpers;
using NopStation.Plugin.Misc.CustomerReminders.Services.ReminderRules;
using NopStation.Plugin.Misc.CustomerReminders.Settings;

namespace NopStation.Plugin.Misc.CustomerReminders.Services;

public class ReminderProcessingService : IReminderProcessingService
{
	private readonly IReminderService _reminderService;

	private readonly IReminderRuleService _reminderRuleService;

	private readonly IReminderReportService _reminderReportService;

	private readonly IEnumerable<IReminderRuleImplementation> _reminderRuleImplementations;

	private readonly IMessageTokenProvider _messageTokenProvider;

	private readonly IMessageTemplateService _messageTemplateService;

	private readonly IQueuedEmailService _queuedEmailService;

	private readonly IEmailAccountService _emailAccountService;

	private readonly ITokenizer _tokenizer;

	private readonly IStoreService _storeService;

	private readonly ICustomerService _customerService;

	private readonly ILanguageService _languageService;

	private readonly IOrderService _orderService;

	private readonly IShoppingCartService _shoppingCartService;

	private readonly IProductService _productService;

	private readonly ISettingService _settingService;

	private readonly ILogger _logger;

	private readonly IVendorService _vendorService;

	public ReminderProcessingService(IReminderService reminderService, IReminderRuleService reminderRuleService, IReminderReportService reminderReportService, IEnumerable<IReminderRuleImplementation> reminderRuleImplementations, IMessageTokenProvider messageTokenProvider, IMessageTemplateService messageTemplateService, IQueuedEmailService queuedEmailService, IEmailAccountService emailAccountService, ITokenizer tokenizer, IStoreService storeService, ICustomerService customerService, ILanguageService languageService, IOrderService orderService, IShoppingCartService shoppingCartService, IProductService productService, ISettingService settingService, IVendorService vendorService, ILogger logger)
	{
		_reminderService = reminderService;
		_reminderRuleService = reminderRuleService;
		_reminderReportService = reminderReportService;
		_reminderRuleImplementations = reminderRuleImplementations;
		_messageTokenProvider = messageTokenProvider;
		_messageTemplateService = messageTemplateService;
		_queuedEmailService = queuedEmailService;
		_emailAccountService = emailAccountService;
		_tokenizer = tokenizer;
		_storeService = storeService;
		_customerService = customerService;
		_languageService = languageService;
		_orderService = orderService;
		_shoppingCartService = shoppingCartService;
		_productService = productService;
		_settingService = settingService;
		_vendorService = vendorService;
		_logger = logger;
	}

	protected virtual int ConvertToMinutes(int value, int intervalTypeId)
	{
		return (IntervalType)intervalTypeId switch
		{
			IntervalType.Minutes => value, 
			IntervalType.Hours => value * 60, 
			IntervalType.Days => value * 60 * 24, 
			_ => value, 
		};
	}

	public virtual async Task ProcessRemindersAsync()
	{
		try
		{
			IPagedList<Reminder> reminders = await _reminderService.GetAllRemindersAsync(null, 0, true);
			if (!reminders.Any())
			{
				await _logger.InformationAsync("ReminderProcessingService: No enabled reminders found");
				return;
			}
			await _logger.InformationAsync($"ReminderProcessingService: Processing {reminders.Count} enabled reminders");
			foreach (Reminder reminder in reminders)
			{
				try
				{
					await ProcessSingleReminderAsync(reminder);
				}
				catch (Exception exception)
				{
					await _logger.ErrorAsync($"ReminderProcessingService: Error processing reminder {reminder.Id} ({reminder.Name})", exception);
				}
			}
			await _logger.InformationAsync("ReminderProcessingService: Completed processing all reminders");
		}
		catch (Exception exception2)
		{
			await _logger.ErrorAsync("ReminderProcessingService: Error in ProcessRemindersAsync", exception2);
		}
	}

	public virtual async Task ProcessSingleReminderAsync(Reminder reminder)
	{
		ArgumentNullException.ThrowIfNull(reminder, "reminder");
		CustomerRemindersSettings settings = await _settingService.LoadSettingAsync<CustomerRemindersSettings>(reminder.StoreId);
		if (!settings.IsEnabled)
		{
			await _logger.InformationAsync($"Reminder {reminder.Id}: Plugin is disabled for Store id {reminder.StoreId}. Skipping.");
			return;
		}
		reminder.ExecutedOnUtc = DateTime.UtcNow;
		await _reminderService.UpdateReminderAsync(reminder);
		if (reminder.ReminderRuleId == 0)
		{
			await _logger.WarningAsync($"Reminder {reminder.Id} has no ReminderRule assigned");
			return;
		}
		ReminderRule reminderRule = await _reminderRuleService.GetReminderRuleByIdAsync(reminder.ReminderRuleId);
		if (reminderRule == null)
		{
			await _logger.WarningAsync($"ReminderRule {reminder.ReminderRuleId} not found for Reminder {reminder.Id}");
			return;
		}
		IReminderRuleImplementation ruleImplementation = _reminderRuleImplementations.FirstOrDefault((IReminderRuleImplementation r) => r.SystemName.Equals(reminderRule.SystemName, StringComparison.InvariantCultureIgnoreCase));
		if (ruleImplementation == null)
		{
			ruleImplementation = _reminderRuleImplementations.FirstOrDefault((IReminderRuleImplementation r) => r.SystemName.Equals("Generic", StringComparison.InvariantCultureIgnoreCase));
		}
		if (ruleImplementation == null)
		{
			await _logger.WarningAsync("No rule implementation found for SystemName: " + reminderRule.SystemName);
			return;
		}
		int dateGreaterThanMinutes = ConvertToMinutes(reminder.DateGreaterThan, reminder.DateGreaterThanIntervalTypeId);
		int dateLowerThanMinutes = ConvertToMinutes(reminder.DateLowerThan, reminder.DateLowerThanIntervalTypeId);
		IList<Customer> list = await ruleImplementation.GetEligibleCustomersAsync(reminder, dateGreaterThanMinutes, dateLowerThanMinutes);
		if (!list.Any())
		{
			await _logger.InformationAsync($"No eligible customers found for Reminder {reminder.Id} ({reminder.Name})");
			return;
		}
		if (settings.IsExcludeGuests)
		{
			List<Customer> filteredCustomers = new List<Customer>();
			foreach (Customer customer in list)
			{
				if (!(await _customerService.IsGuestAsync(customer)))
				{
					filteredCustomers.Add(customer);
				}
			}
			list = filteredCustomers;
		}
		if (!list.Any())
		{
			await _logger.InformationAsync($"No eligible customers found for Reminder {reminder.Id} ({reminder.Name}) after filtering");
			return;
		}
		int sentCount = 0;
		int skippedCount = 0;
		foreach (Customer customer in list)
		{
			try
			{
				DateTime? conditionMetDate = await ruleImplementation.GetConditionMetDateAsync(customer);
				if (await ShouldSendReminderAsync(customer, reminder, conditionMetDate))
				{
					if (await SendReminderEmailAsync(customer, reminder, conditionMetDate))
					{
						sentCount++;
					}
					else
					{
						skippedCount++;
					}
				}
				else
				{
					skippedCount++;
				}
			}
			catch (Exception exception)
			{
				await _logger.ErrorAsync($"Error processing customer {customer.Id} for reminder {reminder.Id}", exception);
				skippedCount++;
			}
		}
		await _logger.InformationAsync($"Reminder {reminder.Id} ({reminder.Name}): Sent {sentCount}, Skipped {skippedCount}");
	}

	public virtual async Task<bool> ShouldSendReminderAsync(Customer customer, Reminder reminder, DateTime? conditionMetDate)
	{
		int num = await _reminderReportService.GetSentMessageCountAsync(customer.Id, reminder.Id);
		if (num >= reminder.MaxMessagesPerCustomer)
		{
			return false;
		}
		if (num > 0)
		{
			ReminderReport reminderReport = await _reminderReportService.GetLastSentReportAsync(customer.Id, reminder.Id);
			if (reminderReport != null)
			{
				int num2 = ConvertToMinutes(reminder.IntervalBetweenMessages, reminder.IntervalBetweenMessagesTypeId);
				DateTime dateTime = reminderReport.CreatedOnUtc.AddMinutes(num2);
				if (DateTime.UtcNow < dateTime)
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual async Task<bool> SendReminderEmailAsync(Customer customer, Reminder reminder, DateTime? conditionMetDate)
	{
		Store store;
		Order order;
		Product product;
		Vendor vendor;
		try
		{
			MessageTemplate messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(reminder.MessageTemplateId);
			if (messageTemplate == null || !messageTemplate.IsActive)
			{
				await _logger.WarningAsync($"Message template {reminder.MessageTemplateId} not found or inactive");
				return false;
			}
			IStoreService storeService = _storeService;
			int storeId = ((reminder.StoreId <= 0) ? ((await _storeService.GetAllStoresAsync()).FirstOrDefault()?.Id ?? 0) : reminder.StoreId);
			store = await storeService.GetStoreByIdAsync(storeId);
			if (store == null)
			{
				await _logger.WarningAsync("Store not found");
				return false;
			}
			EmailAccount emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(messageTemplate.EmailAccountId);
			if (emailAccount == null)
			{
				emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
			}
			if (emailAccount == null)
			{
				await _logger.WarningAsync("Email account not found");
				return false;
			}
			int languageId = customer.LanguageId.GetValueOrDefault();
			if (languageId == 0)
			{
				languageId = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault()?.Id ?? 0;
			}
			ReminderRule reminderRule = await _reminderRuleService.GetReminderRuleByIdAsync(reminder.ReminderRuleId);
			if (reminderRule == null)
			{
				await _logger.WarningAsync($"ReminderRule {reminder.ReminderRuleId} not found");
				return false;
			}
			List<string> tokenGroups = ReminderRuleTokenGroupHelper.ParseTokenGroups(reminderRule.AvailableTokens);
			List<Token> tokens = new List<Token>();
			await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);
			await _messageTokenProvider.AddCustomerTokensAsync(tokens, customer);
			order = null;
			product = null;
			vendor = null;
			foreach (string item in tokenGroups)
			{
				switch (item)
				{
				case "Order tokens":
					if (await GetOrderAsync() != null)
					{
						await _messageTokenProvider.AddOrderTokensAsync(tokens, order, languageId);
					}
					break;
				case "Refunded order  tokens":
					if (await GetOrderAsync() != null)
					{
						await _messageTokenProvider.AddOrderRefundedTokensAsync(tokens, order, 0m);
					}
					break;
				case "Order note tokens":
					await _messageTokenProvider.AddOrderTokensAsync(tokens, order, languageId);
					break;
				case "Vendor tokens":
					if (await GetVendorAsync() != null)
					{
						await _messageTokenProvider.AddVendorTokensAsync(tokens, vendor);
					}
					break;
				case "Product tokens":
				case "Back in stock tokens":
					if (await GetProductAsync() != null)
					{
						await _messageTokenProvider.AddProductTokensAsync(tokens, product, languageId);
					}
					break;
				default:
					await _logger.WarningAsync($"Unknown token group '{item}' in ReminderRule {reminderRule.Id}");
					break;
				case "Store tokens":
				case "Customer tokens":
					break;
				}
			}
			string subject = _tokenizer.Replace(messageTemplate.Subject, tokens, htmlEncode: true);
			string body = _tokenizer.Replace(messageTemplate.Body, tokens, htmlEncode: true);
			QueuedEmail queuedEmail = new QueuedEmail
			{
				Priority = QueuedEmailPriority.High,
				From = emailAccount.Email,
				FromName = emailAccount.DisplayName,
				To = customer.Email
			};
			QueuedEmail queuedEmail2 = queuedEmail;
			queuedEmail2.ToName = await _customerService.GetCustomerFullNameAsync(customer);
			queuedEmail.Bcc = messageTemplate.BccEmailAddresses;
			queuedEmail.Subject = subject;
			queuedEmail.Body = body;
			queuedEmail.CreatedOnUtc = DateTime.UtcNow;
			queuedEmail.EmailAccountId = emailAccount.Id;
			await _queuedEmailService.InsertQueuedEmailAsync(queuedEmail);
			ReminderReport reminderReport = new ReminderReport
			{
				ReminderId = reminder.Id,
				ReminderName = reminder.Name,
				CustomerId = customer.Id
			};
			ReminderReport reminderReport2 = reminderReport;
			reminderReport2.CustomerName = await _customerService.GetCustomerFullNameAsync(customer);
			reminderReport.CustomerEmail = customer.Email;
			reminderReport.StoreId = store.Id;
			reminderReport.StoreName = store.Name;
			reminderReport.CreatedOnUtc = DateTime.UtcNow;
			reminderReport.IsMessageSent = true;
			await _reminderReportService.InsertReminderReportAsync(reminderReport);
			return true;
		}
		catch (Exception exception)
		{
			await _logger.ErrorAsync($"Error sending reminder email to customer {customer.Id}", exception);
			ReminderReport reminderReport3 = new ReminderReport
			{
				ReminderId = reminder.Id,
				ReminderName = reminder.Name,
				CustomerId = customer.Id
			};
			ReminderReport reminderReport = reminderReport3;
			reminderReport.CustomerName = await _customerService.GetCustomerFullNameAsync(customer);
			reminderReport3.CustomerEmail = customer.Email;
			reminderReport3.StoreId = reminder.StoreId;
			ReminderReport reminderReport2 = reminderReport3;
			reminderReport2.StoreName = (await _storeService.GetStoreByIdAsync(reminder.StoreId)).Name;
			reminderReport3.CreatedOnUtc = DateTime.UtcNow;
			reminderReport3.IsMessageSent = false;
			await _reminderReportService.InsertReminderReportAsync(reminderReport3);
			return false;
		}
		async Task<Order> GetOrderAsync()
		{
			if (order != null)
			{
				return order;
			}
			order = (await _orderService.SearchOrdersAsync(0, 0, customer.Id, 0, 0, 0, 0, null, null, null, null, null, null, null, null, "", null, 0, 1)).FirstOrDefault();
			return order;
		}
		async Task<Product> GetProductAsync()
		{
			if (product != null)
			{
				return product;
			}
			int num = (await _shoppingCartService.GetShoppingCartAsync(customer, (ShoppingCartType?)ShoppingCartType.ShoppingCart, store.Id, (int?)null, (DateTime?)null, (DateTime?)null)).FirstOrDefault()?.ProductId ?? 0;
			if (num > 0)
			{
				product = await _productService.GetProductByIdAsync(num);
			}
			return product;
		}
		async Task<Vendor> GetVendorAsync()
		{
			if (vendor != null)
			{
				return vendor;
			}
			vendor = await _vendorService.GetVendorByIdAsync(customer.VendorId);
			return vendor;
		}
	}
}
