using FluentValidation;
using Nop.Plugin.AIPoweredRecommendation.GoogleAI.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.AIPoweredRecommendation.GoogleAI.Validators;

/// <summary>
/// Represents an <see cref="ConfigurationModel"/> validator.
/// </summary>
public class RequirementModelValidator : BaseNopValidator<ConfigurationModel>
{
    public RequirementModelValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.ProjectId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.AIPoweredRecommendation.GoogleAI.ProjectId.Required"));

        RuleFor(model => model.LocationId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.AIPoweredRecommendation.GoogleAI.LocationId.Required"));

        RuleFor(model => model.CatalogId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.AIPoweredRecommendation.GoogleAI.CatalogId.Required"));

        RuleFor(model => model.BranchId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.AIPoweredRecommendation.GoogleAI.BranchId.Required"));
    }
}