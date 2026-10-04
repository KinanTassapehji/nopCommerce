using FluentValidation;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Customer;

namespace Nop.Web.Validators.Customer;

public partial class PasswordRecoveryValidator : BaseNopValidator<PasswordRecoveryModel>
{
    public PasswordRecoveryValidator(ILocalizationService localizationService)
    {
        //by phone or by email: one of the two
        RuleFor(x => x.Email).NotEmpty().When(x => string.IsNullOrEmpty(x.Phone))
            .WithMessageAwait(localizationService.GetResourceAsync("Account.PasswordRecovery.Email.Required"));
        RuleFor(x => x.Email)
            .IsEmailAddress()
            .WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));
        RuleFor(x => x.Phone)
            .Must((x, phone) => string.IsNullOrEmpty(phone) || CustomerPhoneHelper.ToE164(phone, x.PhoneCountry) != null)
            .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));
    }
}