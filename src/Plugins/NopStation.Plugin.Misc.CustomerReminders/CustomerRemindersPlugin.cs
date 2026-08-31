using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders;

public class CustomerRemindersPlugin : BasePlugin, IMiscPlugin, IPlugin, INopStationPlugin
{
	private readonly IRepository<ReminderRule> _reminderRuleRepository;

	private readonly IWebHelper _webHelper;

	private readonly IScheduleTaskService _scheduleTaskService;

	public bool HideInWidgetList => false;

	public CustomerRemindersPlugin(IRepository<ReminderRule> reminderRuleRepository, IWebHelper webHelper, IScheduleTaskService scheduleTaskService)
	{
		_reminderRuleRepository = reminderRuleRepository;
		_webHelper = webHelper;
		_scheduleTaskService = scheduleTaskService;
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/CustomerReminders/Configure";
	}

	public override async Task InstallAsync()
	{
		await this.InstallPluginAsync();
		if (!(await _reminderRuleRepository.GetAllAsync((IQueryable<ReminderRule> query) => query)).Any())
		{
			List<ReminderRule> entities = new List<ReminderRule>
			{
				new ReminderRule
				{
					SystemName = "InactiveCustomers",
					Description = "Target customers who registered but haven't activated their account",
					IsEnabled = true,
					AvailableTokens = TokenGroupNames.CustomerTokens + "," + TokenGroupNames.StoreTokens,
					RuleType = "Built-in",
					CreatedOnUtc = DateTime.UtcNow,
					Deleted = false
				},
				new ReminderRule
				{
					SystemName = "AbandonedCart",
					Description = "Target customers with items in cart who haven't completed purchase",
					IsEnabled = true,
					AvailableTokens = $"{TokenGroupNames.CustomerTokens},{TokenGroupNames.StoreTokens},{TokenGroupNames.ProductTokens}",
					RuleType = "Built-in",
					CreatedOnUtc = DateTime.UtcNow,
					Deleted = false
				},
				new ReminderRule
				{
					SystemName = "UnpaidOrders",
					Description = "Target customers with pending payment orders",
					IsEnabled = true,
					AvailableTokens = $"{TokenGroupNames.CustomerTokens},{TokenGroupNames.StoreTokens},{TokenGroupNames.OrderTokens}",
					RuleType = "Built-in",
					CreatedOnUtc = DateTime.UtcNow,
					Deleted = false
				},
				new ReminderRule
				{
					SystemName = "CompletedOrder",
					Description = "Target customers who completed an order (for review requests, feedback)",
					IsEnabled = true,
					AvailableTokens = $"{TokenGroupNames.CustomerTokens},{TokenGroupNames.StoreTokens},{TokenGroupNames.OrderTokens},{TokenGroupNames.ProductReviewTokens}",
					RuleType = "Built-in",
					CreatedOnUtc = DateTime.UtcNow,
					Deleted = false
				},
				new ReminderRule
				{
					SystemName = "Birthday",
					Description = "Target customers whose birthday is today or within time range",
					IsEnabled = true,
					AvailableTokens = TokenGroupNames.CustomerTokens + "," + TokenGroupNames.StoreTokens,
					RuleType = "Built-in",
					CreatedOnUtc = DateTime.UtcNow,
					Deleted = false
				}
			};
			await _reminderRuleRepository.InsertAsync(entities);
		}
		if (await _scheduleTaskService.GetTaskByTypeAsync("NopStation.Plugin.Misc.CustomerReminders.Tasks.ReminderProcessingTask") == null)
		{
			await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
			{
				Name = "Process Customer Reminders",
				Seconds = 3600,
				Type = "NopStation.Plugin.Misc.CustomerReminders.Tasks.ReminderProcessingTask",
				Enabled = true,
				StopOnError = false,
				LastEnabledUtc = DateTime.UtcNow
			});
		}
		await base.InstallAsync();
	}

	public override async Task UninstallAsync()
	{
		ScheduleTask scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync("NopStation.Plugin.Misc.CustomerReminders.Tasks.ReminderProcessingTask");
		if (scheduleTask != null)
		{
			await _scheduleTaskService.DeleteTaskAsync(scheduleTask);
		}
		await this.UninstallPluginAsync(new CustomerRemindersPermissionConfigManager());
		await base.UninstallAsync();
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["NopStation.Plugins.CustomerReminders.Admin.Menu.CustomerReminders"] = "Customer Reminders",
			["NopStation.Plugins.CustomerReminders.Admin.Menu.Configuration"] = "Configuration",
			["NopStation.Plugins.CustomerReminders.Admin.Menu.ReminderRules"] = "Reminder rules",
			["NopStation.Plugins.CustomerReminders.Admin.Menu.Reminders"] = "Reminders",
			["NopStation.Plugins.CustomerReminders.Admin.Menu.ReminderReports"] = "Reminder reports",
			["NopStation.Plugins.CustomerReminders.Admin.Configuration.Title"] = "Customer reminders - Configuration",
			["NopStation.Plugins.CustomerReminders.Admin.Configuration.GeneralSettings"] = "General settings",
			["NopStation.Plugins.CustomerReminders.Admin.Configuration.Saved"] = "Configuration saved successfully.",
			["NopStation.Plugins.CustomerReminders.Admin.Configuration.Fields.Enabled"] = "Enable plugin",
			["NopStation.Plugins.CustomerReminders.Admin.Configuration.Fields.Enabled.Hint"] = "Check to enable the Customer Reminders plugin. Unchecking will disable all reminder processing.",
			["NopStation.Plugins.CustomerReminders.Admin.Configuration.Fields.ExcludeGuests"] = "Exclude guest customers",
			["NopStation.Plugins.CustomerReminders.Admin.Configuration.Fields.ExcludeGuests.Hint"] = "Check to exclude guest customers from all reminders. Only registered customers will receive reminders.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Title"] = "Customer reminders",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.AddNew"] = "Add new reminder",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.BackToList"] = "back to reminder list",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.EditDetails"] = "Edit ",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Edit.Title"] = "Edit customer reminder",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Create.Title"] = "Add new customer reminder",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders"] = "Reminders",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Alert.Validation.Failed"] = "Validation failed. Please check the form for errors and try again.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.List.SearchReminderName"] = "Reminder name",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.List.SearchReminderName.Hint"] = "Search by reminder name",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.List.SearchEnabled"] = "Enabled",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.List.SearchEnabled.Hint"] = "Filter by enabled status",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.List.SearchEnabled.EnabledOnly"] = "Enabled only",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.List.SearchEnabled.DisabledOnly"] = "Disabled only",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Name"] = "Name",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Name.Hint"] = "The name of the reminder.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Name.Required"] = "Reminder name is required.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Name.Unique"] = "Reminder name must be unique.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.IsEnabled"] = "Enabled",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.IsEnabled.Hint"] = "Check to enable this reminder.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.ReminderRule"] = "Reminder rule",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.ReminderRule.Hint"] = "Select the reminder rule to use. This determines which customers will be targeted and which tokens are available.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.AvailableTokens"] = "Available tokens",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.AvailableTokens.Hint"] = "These tokens can be used in the message template.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.EmailAccount"] = "Email account",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.EmailAccount.Hint"] = "The email account to use for sending this reminder.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MaxMessagesPerCustomer"] = "Max messages per customer",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MaxMessagesPerCustomer.Hint"] = "The maximum number of messages to send per customer.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MaxMessagesPerCustomer.Positive"] = "Maximum messages per customer must be a positive number.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.IntervalBetweenMessages"] = "Interval between messages",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.IntervalBetweenMessages.Hint"] = "The interval between messages.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.IntervalBetweenMessages.Positive"] = "Interval between messages must be a positive number.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.IntervalBetweenMessagesType"] = "Interval type",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.IntervalBetweenMessagesType.Hint"] = "Select the interval type (Minutes, Hours, or Days).",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateGreaterThan"] = "Time greater than",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateGreaterThan.Hint"] = "The condition met time should be greater than or equal to this value.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateGreaterThan.Positive"] = "Time greater than must be a positive number.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateGreaterThanIntervalType"] = "Interval type",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateGreaterThanIntervalType.Hint"] = "Select the interval type (Minutes, Hours, or Days).",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateLowerThan"] = "Time lower than",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateLowerThan.Hint"] = "The condition met time should be lower than or equal to this value.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateLowerThan.MustBeGreaterThanDateGreaterThan"] = "Time lower than must be greater than time greater than.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateLowerThanIntervalType"] = "Interval type",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateLowerThanIntervalType.Hint"] = "Select the interval type (Minutes, Hours, or Days).",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Store"] = "Store",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Store.Hint"] = "Select the store for this reminder.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Vendor"] = "Vendor",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Vendor.Hint"] = "Select the vendor for this reminder.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.ExecutedOnUtc"] = "Executed on",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.ExecutedOnUtc.Hint"] = "The date and time (in UTC) when the reminder was last executed.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.MessageTemplate"] = "Message template",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Name"] = "Template name",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Name.Hint"] = "The name of the message template (for internal reference).",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Name.Required"] = "Message template name is required.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Bcc"] = "BCC",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Bcc.Hint"] = "Blind carbon copy. Semicolon separated list of recipient email addresses.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Bcc.Required"] = "BCC field is required. Enter at least one email address.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Bcc.Invalid"] = "One or more email addresses in the BCC field are invalid.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Subject"] = "Subject",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Subject.Hint"] = "The subject of the email. Use allowed tokens from the selected reminder rule.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Subject.Required"] = "Message template subject is required.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Body"] = "Body",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Body.Hint"] = "The body of the email. Use allowed tokens from the selected reminder rule.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Body.Required"] = "Message template body is required.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateLowerThan.MustBeGreaterThanTimeGreaterThan"] = "Time lower than must be greater than time greater than.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Tabs.Settings"] = "Settings",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Tabs.MessageTemplate"] = "Message template",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Tabs.ExcludedCustomers"] = "Excluded customers",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Notifications.Added"] = "A new reminder has been added successfully.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Notifications.Updated"] = "The reminder has been updated successfully.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.Notifications.Deleted"] = "The reminder has been deleted successfully.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.ExcludedCustomers.Description"] = "The customers listed here will be excluded from receiving this reminder.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.ExcludedCustomers.AddNew"] = "Add new excluded customer",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.ExcludedCustomers.Fields.CustomerEmail"] = "Customer email",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.ExcludedCustomers.Fields.CustomerEmail.Hint"] = "Enter the email address of the customer to exclude.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.ExcludedCustomers.Fields.CustomerEmail.Required"] = "Customer email is required.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.ExcludedCustomers.Fields.CustomerEmail.NotFound"] = "No customer found with the provided email address.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.ExcludedCustomers.Fields.CustomerName"] = "Customer name",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.ExcludedCustomers.Fields.CustomerName.Hint"] = "The name of the excluded customer.",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.ExcludedCustomers.Search.Email"] = "Customer email",
			["NopStation.Plugins.CustomerReminders.Admin.Reminders.ExcludedCustomers.Search.Email.Hint"] = "Search by customer email address.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Title"] = "Reminder rules",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.AddNew"] = "Add new rule",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.AddNew.Title"] = "Add new reminder rule",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.BackToList"] = "back to rule list",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.EditDetails.Title"] = "Edit reminder rule",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.EditDetails"] = "Edit",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Edit.Title"] = "Edit reminder rule",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Info"] = "Info",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.SystemName"] = "System name",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.SystemName.Hint"] = "The unique system name of the reminder rule.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.SystemName.Required"] = "Reminder rule system name is required.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.SystemName.Unique"] = "Reminder rule system name must be unique.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.Description"] = "Description",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.Description.Hint"] = "The description of the reminder rule.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.IsEnabled"] = "Enabled",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.IsEnabled.Hint"] = "Check to enable this reminder rule.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.AvailableTokens"] = "Available tokens",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.AvailableTokens.Hint"] = "Select the tokens that are available for this reminder rule. These tokens can be used in message templates.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.AllowedTokensDisplay"] = "Allowed tokens (display)",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.AllowedTokensDisplay.Hint"] = "This shows the actual tokens that will be available based on your selection above.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.RuleType"] = "Rule type",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.RuleType.Hint"] = "Indicates whether this is a built-in rule with hardcoded logic or a custom rule created by admin.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.CreatedOnUtc"] = "Created on",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.UpdatedOnUtc"] = "Updated on",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Notifications.Added"] = "A new reminder rule has been added successfully.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Notifications.Updated"] = "The reminder rule has been updated successfully.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Notifications.Deleted"] = "The reminder rule has been deleted successfully.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Notifications.CannotDeleteSystemRule"] = "Cannot delete a system/built-in reminder rule.",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Title"] = "Reminder reports",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.NoReports"] = "No reminder reports selected",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.ReminderId"] = "Reminder ID",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.ReminderId.Hint"] = "Enter the reminder ID to find reports",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.ReminderName"] = "Reminder name",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.ReminderName.Hint"] = "Enter the reminder name to find reports",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CustomerId"] = "Customer ID",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CustomerId.Hint"] = "Enter the customer ID to find reports",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CustomerName"] = "Customer name",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CustomerName.Hint"] = "Enter the customer name to find reports",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CustomerEmail"] = "Customer email",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CustomerEmail.Hint"] = "Enter the customer email to find reports",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.StoreId"] = "Store ID",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.StoreId.Hint"] = "Enter the store ID to find reports",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.StoreName"] = "Store name",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.StoreName.Hint"] = "Select the store to find reports",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.IsMessageSent"] = "Message sent",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.IsMessageSent.Hint"] = "Select whether the message was sent to find reports",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CreatedFrom"] = "Created from",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CreatedFrom.Hint"] = "Search for reports created from this date",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CreatedTo"] = "Created to",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Search.CreatedTo.Hint"] = "Search for reports created to this date",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.ReminderName"] = "Reminder name",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.CustomerName"] = "Customer name",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.CustomerEmail"] = "Customer email",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.StoreName"] = "Store name",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.IsMessageSent"] = "Message sent",
			["NopStation.Plugins.CustomerReminders.Admin.ReminderReports.Fields.CreatedOn"] = "Created on",
			["NopStation.Plugins.CustomerReminders.Admin.Validation.Failed"] = "Please fill in all required fields correctly before saving."
		};
	}
}
