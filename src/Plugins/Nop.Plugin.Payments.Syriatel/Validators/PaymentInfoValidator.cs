using FluentValidation;
using Nop.Plugin.Payments.Syriatel.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.Syriatel.Validators;

public class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
{
    public PaymentInfoValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.CustomerMsisdn).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.Required"));
        RuleFor(x => x.CustomerMsisdn).Matches(@"^09\d{8}$").WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));
    }
}
