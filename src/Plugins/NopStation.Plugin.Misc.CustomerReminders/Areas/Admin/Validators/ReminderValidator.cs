using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Domains.Enums;
using NopStation.Plugin.Misc.CustomerReminders.Helpers;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Validators;

public class ReminderValidator : BaseNopValidator<ReminderModel>
{
	public ReminderValidator(ILocalizationService localizationService)
	{
		RuleFor((ReminderModel x) => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.Name.Required"));
		RuleFor((ReminderModel x) => x.MaxMessagesPerCustomer).GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MaxMessagesPerCustomer.Positive"));
		RuleFor((ReminderModel x) => x.IntervalBetweenMessages).GreaterThan(0).When((ReminderModel x) => x.MaxMessagesPerCustomer > 1).WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.IntervalBetweenMessages.Positive"));
		RuleFor((ReminderModel x) => x.DateGreaterThan).GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateGreaterThan.Positive"));
		RuleFor((ReminderModel x) => x.MessageTemplateName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Name.Required"));
		RuleFor((ReminderModel x) => x.MessageTemplateSubject).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Subject.Required"));
		RuleFor((ReminderModel x) => x.MessageTemplateBody).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Body.Required"));
		RuleFor((ReminderModel x) => x.MessageTemplateBcc).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Bcc.Required"));
		RuleFor((ReminderModel x) => x.MessageTemplateBcc).Must(EmailValidationHelper.AreValidEmails).When((ReminderModel x) => !string.IsNullOrWhiteSpace(x.MessageTemplateBcc)).WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.MessageTemplate.Bcc.Invalid"));
		RuleFor((ReminderModel x) => x.DateLowerThan).Must(delegate(ReminderModel model, int dateLowerThan)
		{
			if (dateLowerThan <= 0)
			{
				return false;
			}
			int num = ConvertToMinutes(model.DateGreaterThan, model.DateGreaterThanIntervalTypeId);
			return ConvertToMinutes(dateLowerThan, model.DateLowerThanIntervalTypeId) > num;
		}).WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.Reminders.Fields.DateLowerThan.MustBeGreaterThanTimeGreaterThan"));
	}

	private static int ConvertToMinutes(int value, int intervalTypeId)
	{
		return (IntervalType)intervalTypeId switch
		{
			IntervalType.Minutes => value, 
			IntervalType.Hours => value * 60, 
			IntervalType.Days => value * 60 * 24, 
			_ => value, 
		};
	}
}
