using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Domains.Enums;
using NopStation.Plugin.Misc.CustomerReminders.Helpers;
using NopStation.Plugin.Misc.CustomerReminders.Services;
using NopStation.Plugin.Misc.CustomerReminders.Settings;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;

public class ReminderModelFactory : IReminderModelFactory
{
	private readonly IReminderService _reminderService;

	private readonly ILocalizationService _localizationService;

	private readonly IMessageTemplateService _messageTemplateService;

	private readonly IStoreService _storeService;

	private readonly IVendorService _vendorService;

	private readonly IReminderRuleService _reminderRuleService;

	private readonly IMessageTokenProvider _messageTokenProvider;

	private readonly IReminderExcludedCustomerService _reminderExcludedCustomerService;

	private readonly ICustomerService _customerService;

	private readonly IEmailAccountService _emailAccountService;

	private readonly ISettingService _settingService;

	public ReminderModelFactory(IReminderService reminderService, ILocalizationService localizationService, IMessageTemplateService messageTemplateService, IReminderExcludedCustomerService reminderExcludedCustomerService, IStoreService storeService, ICustomerService customerService, IReminderRuleService reminderRuleService, IMessageTokenProvider messageTokenProvider, IEmailAccountService emailAccountService, IVendorService vendorService, ISettingService settingService)
	{
		_reminderService = reminderService;
		_localizationService = localizationService;
		_messageTemplateService = messageTemplateService;
		_reminderRuleService = reminderRuleService;
		_messageTokenProvider = messageTokenProvider;
		_reminderExcludedCustomerService = reminderExcludedCustomerService;
		_customerService = customerService;
		_storeService = storeService;
		_emailAccountService = emailAccountService;
		_vendorService = vendorService;
		_settingService = settingService;
	}

	public virtual async Task<ReminderSearchModel> PrepareReminderSearchModelAsync(ReminderSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		IList<SelectListItem> availableEnabledOptions = searchModel.AvailableEnabledOptions;
		SelectListItem selectListItem = new SelectListItem();
		SelectListItem selectListItem2 = selectListItem;
		selectListItem2.Text = await _localizationService.GetResourceAsync("Admin.Common.All");
		selectListItem.Value = "0";
		availableEnabledOptions.Add(selectListItem);
		availableEnabledOptions = searchModel.AvailableEnabledOptions;
		selectListItem2 = new SelectListItem();
		selectListItem = selectListItem2;
		selectListItem.Text = await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.List.SearchEnabled.EnabledOnly");
		selectListItem2.Value = "1";
		availableEnabledOptions.Add(selectListItem2);
		availableEnabledOptions = searchModel.AvailableEnabledOptions;
		selectListItem = new SelectListItem();
		selectListItem2 = selectListItem;
		selectListItem2.Text = await _localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.List.SearchEnabled.DisabledOnly");
		selectListItem.Value = "2";
		availableEnabledOptions.Add(selectListItem);
		searchModel.SetGridPageSize();
		return searchModel;
	}

	public virtual async Task<ReminderListModel> PrepareReminderListModelAsync(ReminderSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		bool? isEnabled = ((searchModel.SearchEnabledId == 0) ? ((bool?)null) : new bool?(searchModel.SearchEnabledId == 1));
		IPagedList<Reminder> reminders = await _reminderService.GetAllRemindersAsync(searchModel.SearchReminderName, 0, isEnabled, searchModel.Page - 1, searchModel.PageSize);
		return new ReminderListModel().PrepareToGrid(searchModel, reminders, () => reminders.Select((Reminder reminder) => reminder.ToModel<ReminderModel>()));
	}

	public virtual async Task<ReminderModel> PrepareReminderModelAsync(ReminderModel model, Reminder reminder, bool excludeProperties = false)
	{
		if (model == null)
		{
			if (reminder != null && !excludeProperties)
			{
				model = reminder.ToModel<ReminderModel>();
				if (reminder.MessageTemplateId > 0)
				{
					MessageTemplate messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(reminder.MessageTemplateId);
					if (messageTemplate != null)
					{
						model.MessageTemplateName = messageTemplate.Name;
						model.MessageTemplateBcc = messageTemplate.BccEmailAddresses;
						model.MessageTemplateSubject = messageTemplate.Subject;
						model.MessageTemplateBody = messageTemplate.Body;
						model.EmailAccountId = messageTemplate.EmailAccountId;
					}
				}
			}
			else
			{
				model = new ReminderModel();
			}
		}
		if (reminder == null)
		{
			model.MaxMessagesPerCustomer = 2;
			model.DateGreaterThan = 1;
			model.DateLowerThan = 30;
			model.IntervalBetweenMessages = 2;
			model.DateGreaterThanIntervalTypeId = 10;
			model.DateLowerThanIntervalTypeId = 30;
			model.IntervalBetweenMessagesTypeId = 10;
		}
		model.AvailableReminderRules = (await _reminderRuleService.GetAllReminderRulesAsync()).Select((ReminderRule rr) => new SelectListItem
		{
			Text = rr.SystemName,
			Value = rr.Id.ToString(),
			Selected = (reminder != null && reminder.ReminderRuleId == rr.Id)
		}).ToList();
		Reminder reminder2 = reminder;
		int num;
		if (reminder2 == null)
		{
			num = 0;
		}
		else
		{
			_ = reminder2.ReminderRuleId;
			num = 1;
		}
		if (num != 0 && reminder.ReminderRuleId > 0)
		{
			ReminderRule reminderRule = await _reminderRuleService.GetReminderRuleByIdAsync(reminder.ReminderRuleId);
			if (reminderRule != null && !string.IsNullOrEmpty(reminderRule.AvailableTokens))
			{
				List<string> tokenGroups = ReminderRuleTokenGroupHelper.ParseTokenGroups(reminderRule.AvailableTokens);
				model.AvailableTokensFromRule = string.Join(", ", await _messageTokenProvider.GetListOfAllowedTokensAsync(tokenGroups));
			}
		}
		List<SelectListItem> availableDateLowerThanIntervalTypes = (List<SelectListItem>)(model.AvailableDateGreaterThanIntervalTypes = (model.AvailableIntervalBetweenMessagesTypes = Enum.GetValues<IntervalType>().Select(delegate(IntervalType t)
		{
			SelectListItem obj = new SelectListItem
			{
				Text = t.ToString()
			};
			int num2 = (int)t;
			obj.Value = num2.ToString();
			return obj;
		}).ToList()));
		model.AvailableDateLowerThanIntervalTypes = availableDateLowerThanIntervalTypes;
		model.AvailableEmailAccounts = (await _emailAccountService.GetAllEmailAccountsAsync()).Select((EmailAccount ea) => new SelectListItem
		{
			Text = ea.DisplayName + " (" + ea.Email + ")",
			Value = ea.Id.ToString()
		}).ToList();
		model.AvailableStores = (await _storeService.GetAllStoresAsync()).Select((Store s) => new SelectListItem
		{
			Text = s.Name,
			Value = s.Id.ToString()
		}).ToList();
		model.AvailableVendors = (await _vendorService.GetAllVendorsAsync()).Select((Vendor v) => new SelectListItem
		{
			Text = v.Name,
			Value = v.Id.ToString()
		}).ToList();
		IList<SelectListItem> availableVendors = model.AvailableVendors;
		SelectListItem selectListItem = new SelectListItem();
		SelectListItem selectListItem2 = selectListItem;
		selectListItem2.Text = await _localizationService.GetResourceAsync("Admin.Common.All");
		selectListItem.Value = "0";
		availableVendors.Insert(0, selectListItem);
		if (reminder != null)
		{
			ReminderModel reminderModel = model;
			reminderModel.ReminderExcludedCustomerSearchModel = await PrepareReminderExcludedCustomerSearchModelAsync(new ReminderExcludedCustomerSearchModel(), reminder);
		}
		return model;
	}

	public virtual async Task<ReminderExcludedCustomerSearchModel> PrepareReminderExcludedCustomerSearchModelAsync(ReminderExcludedCustomerSearchModel searchModel, Reminder reminder)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		ArgumentNullException.ThrowIfNull(reminder, "reminder");
		searchModel.ReminderId = reminder.Id;
		searchModel.SetGridPageSize();
		return await Task.FromResult(searchModel);
	}

	public virtual async Task<ReminderExcludedCustomerListModel> PrepareReminderExcludedCustomerListModelAsync(ReminderExcludedCustomerSearchModel searchModel, Reminder reminder)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		ArgumentNullException.ThrowIfNull(reminder, "reminder");
		IReminderExcludedCustomerService reminderExcludedCustomerService = _reminderExcludedCustomerService;
		int? reminderId = reminder.Id;
		int pageIndex = searchModel.Page - 1;
		int pageSize = searchModel.PageSize;
		IPagedList<ReminderExcludedCustomer> excludedCustomers = await reminderExcludedCustomerService.GetAllReminderExcludedCustomersAsync(reminderId, null, pageIndex, pageSize);
		return await new ReminderExcludedCustomerListModel().PrepareToGridAsync(searchModel, excludedCustomers, () => excludedCustomers.SelectAwait<ReminderExcludedCustomer, ReminderExcludedCustomerModel>(async delegate(ReminderExcludedCustomer excludedCustomer)
		{
			Customer customer = await _customerService.GetCustomerByIdAsync(excludedCustomer.CustomerId);
			ReminderExcludedCustomerModel reminderExcludedCustomerModel = new ReminderExcludedCustomerModel
			{
				Id = excludedCustomer.Id,
				ReminderId = excludedCustomer.ReminderId,
				CustomerId = excludedCustomer.CustomerId,
				CustomerEmail = (customer?.Email ?? string.Empty)
			};
			ReminderExcludedCustomerModel reminderExcludedCustomerModel2 = reminderExcludedCustomerModel;
			reminderExcludedCustomerModel2.CustomerName = await _customerService.GetCustomerFullNameAsync(customer);
			return reminderExcludedCustomerModel;
		}));
	}

	public virtual Task<AddCustomerToReminderSearchModel> PrepareAddCustomerToReminderSearchModelAsync(AddCustomerToReminderSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		searchModel.SetGridPageSize();
		return Task.FromResult(searchModel);
	}

	public virtual async Task<AddCustomerToReminderListModel> PrepareAddCustomerToReminderListModelAsync(AddCustomerToReminderSearchModel searchModel, int reminderId)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		Reminder reminder = await _reminderService.GetReminderByIdAsync(reminderId);
		CustomerRemindersSettings obj = await _settingService.LoadSettingAsync<CustomerRemindersSettings>(reminder.StoreId);
		int[] customerRoleIds = null;
		if (obj.IsExcludeGuests)
		{
			CustomerRole guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);
			IList<CustomerRole> source = await _customerService.GetAllCustomerRolesAsync();
			if (guestRole != null)
			{
				customerRoleIds = (from r in source
					where r.Id != guestRole.Id
					select r.Id).ToArray();
			}
		}
		ICustomerService customerService = _customerService;
		string searchEmail = searchModel.SearchEmail;
		int[] customerRoleIds2 = customerRoleIds;
		int pageIndex = searchModel.Page - 1;
		int pageSize = searchModel.PageSize;
		IPagedList<Customer> customers = await customerService.GetAllCustomersAsync(null, null, null, null, 0, 0, customerRoleIds2, searchEmail, null, null, null, 0, 0, null, null, null, null, null, pageIndex, pageSize);
		return await new AddCustomerToReminderListModel().PrepareToGridAsync(searchModel, customers, () => customers.SelectAwait<Customer, AddCustomerToReminderModel>(async delegate(Customer customer)
		{
			string customerRoleNames = string.Join(", ", (await _customerService.GetCustomerRolesAsync(customer)).Select((CustomerRole role) => role.Name));
			AddCustomerToReminderModel addCustomerToReminderModel = new AddCustomerToReminderModel
			{
				Id = customer.Id,
				Email = customer.Email,
				Username = customer.Username
			};
			AddCustomerToReminderModel addCustomerToReminderModel2 = addCustomerToReminderModel;
			addCustomerToReminderModel2.FullName = await _customerService.GetCustomerFullNameAsync(customer);
			addCustomerToReminderModel.CustomerRoleNames = customerRoleNames;
			addCustomerToReminderModel.Active = customer.Active;
			addCustomerToReminderModel.ReminderId = reminderId;
			return addCustomerToReminderModel;
		}));
	}
}
