using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Common;

namespace Nop.Web.Validators.Common;

public partial class MaintenanceRequestValidator : BaseNopValidator<MaintenanceRequestModel>
{
    public MaintenanceRequestValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("MaintenanceRequest.FullName.Required"));
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("MaintenanceRequest.PhoneNumber.Required"));
        RuleFor(x => x.PhoneNumber)
            .Matches("^05[0-9]{8}$")
            .WithMessageAwait(localizationService.GetResourceAsync("MaintenanceRequest.PhoneNumber.Invalid"))
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        //the live form leaves the e-mail optional; it is only validated when filled in
        RuleFor(x => x.Email)
            .IsEmailAddress()
            .WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"))
            .When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.City).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("MaintenanceRequest.City.Required"));
        RuleFor(x => x.Area).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("MaintenanceRequest.Area.Required"));
        RuleFor(x => x.Brand).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("MaintenanceRequest.Brand.Required"));
        RuleFor(x => x.DeviceType).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("MaintenanceRequest.DeviceType.Required"));
        RuleFor(x => x.ModelNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("MaintenanceRequest.ModelNumber.Required"));
        RuleFor(x => x.Problem).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("MaintenanceRequest.Problem.Required"));
    }
}