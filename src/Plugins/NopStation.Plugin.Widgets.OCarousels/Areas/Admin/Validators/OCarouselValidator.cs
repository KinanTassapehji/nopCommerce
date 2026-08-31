using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Validators;

public class OCarouselValidator : BaseNopValidator<OCarouselModel>
{
	public OCarouselValidator(ILocalizationService localizationService)
	{
		RuleFor((OCarouselModel x) => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.OCarousels.OCarousels.Fields.Name.Required").Result);
		RuleFor((OCarouselModel x) => x.Title).NotEmpty().When((OCarouselModel x) => x.DisplayTitle).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.OCarousels.OCarousels.Fields.Title.Required").Result);
		RuleFor((OCarouselModel x) => x.NumberOfItemsToShow).GreaterThan(0).When((OCarouselModel x) => x.DataSourceTypeId != 100).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.OCarousels.OCarousels.Fields.NumberOfItemsToShow.Required").Result);
	}
}
