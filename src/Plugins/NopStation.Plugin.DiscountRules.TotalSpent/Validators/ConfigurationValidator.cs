using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.DiscountRules.TotalSpent.Models;

namespace NopStation.Plugin.DiscountRules.TotalSpent.Validators;

public class ConfigurationValidator : BaseNopValidator<RequirementModel>
{
	public ConfigurationValidator(ILocalizationService localizationService)
	{
		RuleFor((RequirementModel model) => model.Amount).InclusiveBetween(1m, 1000000000m).WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.TotalSpent.Fields.Amount.Range"));
		RuleFor((RequirementModel model) => model.Amount).GreaterThan(0m).WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.TotalSpent.Fields.Amount.Required"));
	}
}
