using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Validators;

public class SliderValidator : BaseNopValidator<SliderModel>
{
	public SliderValidator(ILocalizationService localizationService)
	{
		RuleFor((SliderModel x) => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Sliders.Fields.Name.Required").Result);
		RuleFor((SliderModel x) => x.BackgroundPictureId).GreaterThan(0).When((SliderModel x) => x.ShowBackgroundPicture).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Sliders.Fields.BackGroundPicture.Required").Result);
	}
}
