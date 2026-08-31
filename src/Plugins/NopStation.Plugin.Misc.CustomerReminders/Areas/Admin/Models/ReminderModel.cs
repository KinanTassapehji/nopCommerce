using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

public record ReminderModel : BaseNopEntityModel
{
	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Name")]
	public string Name { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.IsEnabled")]
	public bool IsEnabled { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.ReminderRule")]
	public int ReminderRuleId { get; set; }

	public IList<SelectListItem> AvailableReminderRules { get; set; }

	public string AvailableTokensFromRule { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MaxMessagesPerCustomer")]
	public int MaxMessagesPerCustomer { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.ExecutedOnUtc")]
	public DateTime ExecutedOnUtc { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.IntervalBetweenMessagesType")]
	public int IntervalBetweenMessagesTypeId { get; set; }

	public IList<SelectListItem> AvailableIntervalBetweenMessagesTypes { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.IntervalBetweenMessages")]
	public int IntervalBetweenMessages { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateGreaterThanIntervalType")]
	public int DateGreaterThanIntervalTypeId { get; set; }

	public IList<SelectListItem> AvailableDateGreaterThanIntervalTypes { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateGreaterThan")]
	public int DateGreaterThan { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateLowerThanIntervalType")]
	public int DateLowerThanIntervalTypeId { get; set; }

	public IList<SelectListItem> AvailableDateLowerThanIntervalTypes { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateLowerThan")]
	public int DateLowerThan { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Store")]
	public int StoreId { get; set; }

	public IList<SelectListItem> AvailableStores { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Vendor")]
	public int VendorId { get; set; }

	public IList<SelectListItem> AvailableVendors { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Name")]
	public string MessageTemplateName { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Bcc")]
	public string MessageTemplateBcc { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Subject")]
	public string MessageTemplateSubject { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Body")]
	public string MessageTemplateBody { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.EmailAccount")]
	public int EmailAccountId { get; set; }

	public IList<SelectListItem> AvailableEmailAccounts { get; set; }

	public ReminderExcludedCustomerSearchModel ReminderExcludedCustomerSearchModel { get; set; }

	public ReminderModel()
	{
		AvailableStores = new List<SelectListItem>();
		AvailableIntervalBetweenMessagesTypes = new List<SelectListItem>();
		AvailableDateGreaterThanIntervalTypes = new List<SelectListItem>();
		AvailableDateLowerThanIntervalTypes = new List<SelectListItem>();
		AvailableReminderRules = new List<SelectListItem>();
		AvailableEmailAccounts = new List<SelectListItem>();
		ReminderExcludedCustomerSearchModel = new ReminderExcludedCustomerSearchModel();
	}
}
