using FluentValidation;
using Nop.Plugin.Payments.MtnCash.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.MtnCash.Validators;

public class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
{
    public PaymentInfoValidator(ILocalizationService localizationService)
    {

        RuleFor(x => x.Phone).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.Required"));
        RuleFor(x => x.Phone).Matches(@"^09\d{8}$").WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));
    }
}