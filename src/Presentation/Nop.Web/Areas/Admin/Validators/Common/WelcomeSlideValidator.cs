using FluentValidation;
using Nop.Core.Domain.Common;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Common;

public partial class WelcomeSlideValidator : BaseNopValidator<WelcomeSlideModel>
{
    public WelcomeSlideValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Title).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.ContentManagement.WelcomeSlides.Fields.Title.Required"));

        SetDatabaseValidationRules<WelcomeSlide>();
    }
}