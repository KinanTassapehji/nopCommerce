using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.DiscountRules.CartTotal.Models;

namespace NopStation.Plugin.DiscountRules.CartTotal.Validators;

public class ConfigurationValidator : BaseNopValidator<RequirementModel>
{
	public ConfigurationValidator(ILocalizationService localizationService)
	{
		RuleFor((RequirementModel model) => model.MinimumCartTotal).GreaterThan(0m).WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.CartTotal.Fields.MinimumCartTotal.Range"));
	}
}
