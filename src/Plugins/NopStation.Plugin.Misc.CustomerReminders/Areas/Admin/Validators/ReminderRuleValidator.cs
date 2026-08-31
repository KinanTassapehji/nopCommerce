using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Validators;

public class ReminderRuleValidator : BaseNopValidator<ReminderRuleModel>
{
	public ReminderRuleValidator(ILocalizationService localizationService)
	{
		RuleFor((ReminderRuleModel x) => x.SystemName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.SystemName.Required"));
	}
}
