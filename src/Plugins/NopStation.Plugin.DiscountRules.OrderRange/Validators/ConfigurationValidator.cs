using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.DiscountRules.OrderRange.Models;

namespace NopStation.Plugin.DiscountRules.OrderRange.Validators;

public class ConfigurationValidator : BaseNopValidator<RequirementModel>
{
	public ConfigurationValidator(ILocalizationService localizationService)
	{
		RuleFor((RequirementModel model) => model.RangeValue).InclusiveBetween(1, 1000000000).WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.OrderRange.Fields.RangeValue.Range"));
		RuleFor((RequirementModel model) => model.ConditionValue).NotEqual("0").WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.OrderRange.Fields.ConditionValue.Required"));
	}
}
