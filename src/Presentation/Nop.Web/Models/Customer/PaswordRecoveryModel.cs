using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Customer;

public partial record PasswordRecoveryModel : BaseNopModel
{
    [DataType(DataType.EmailAddress)]
    [NopResourceDisplayName("Account.PasswordRecovery.Email")]
    public string Email { get; set; }

    //recovery by a code sent to the phone; shown while phone verification is on
    public bool PhoneEnabled { get; set; }

    [DataType(DataType.PhoneNumber)]
    [NopResourceDisplayName("Account.PasswordRecovery.Phone")]
    public string Phone { get; set; }

    public string PhoneCountry { get; set; }
    public IList<SelectListItem> AvailablePhoneCountries { get; set; } = new List<SelectListItem>();

    public bool DisplayCaptcha { get; set; }
}