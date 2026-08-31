using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Validators;

public class SliderItemValidator : BaseNopValidator<SliderItemModel>
{
	public SliderItemValidator(ILocalizationService localizationService)
	{
		RuleFor((SliderItemModel x) => x.PictureId).GreaterThan(0).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.SliderItems.Fields.Picture.Required").Result);
		RuleFor((SliderItemModel x) => x.MobilePictureId).GreaterThan(0).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.SliderItems.Fields.MobilePicture.Required").Result);
	}
}
