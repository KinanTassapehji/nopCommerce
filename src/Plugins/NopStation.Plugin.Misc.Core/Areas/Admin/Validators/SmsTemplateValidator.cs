using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Validators;

public class SmsTemplateValidator : BaseNopValidator<SmsTemplateModel>
{
	public SmsTemplateValidator(ILocalizationService localizationService)
	{
		RuleFor((SmsTemplateModel x) => x.Body).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.Core.SmsTemplates.Fields.Body.Required").Result);
	}
}
