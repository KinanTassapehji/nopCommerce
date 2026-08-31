using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Validators;

public class LocaleResourceValidator : BaseNopValidator<CoreLocaleResourceModel>
{
	public LocaleResourceValidator()
	{
		RuleFor((CoreLocaleResourceModel x) => x.ResourceName).NotEmpty().WithMessage("Admin.Configuration.Languages.Resources.Fields.Name.Required");
	}
}
