using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.DiscountRules.DaysOfWeek.Models;

namespace NopStation.Plugin.DiscountRules.DaysOfWeek.Validators;

public class RequirementModelValidator : BaseNopValidator<RequirementModel>
{
	public RequirementModelValidator(ILocalizationService localizationService)
	{
		RuleFor((RequirementModel model) => model.DiscountId).GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DiscountId.Required"));
		RuleFor((RequirementModel model) => model.DaysOfWeek).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DaysOfWeek.Required"));
	}
}
