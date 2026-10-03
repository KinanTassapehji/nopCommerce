using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Customer;

public partial record LoginModel : BaseNopModel
{
    public bool CheckoutAsGuest { get; set; }

    [DataType(DataType.EmailAddress)]
    [NopResourceDisplayName("Account.Login.Fields.Email")]
    public string Email { get; set; }

    public bool UsernamesEnabled { get; set; }

    public UserRegistrationType RegistrationType { get; set; }

    [NopResourceDisplayName("Account.Login.Fields.Username")]
    public string Username { get; set; }

    //two-letter ISO code of the country picked beside the phone number (the username)
    public string PhoneCountry { get; set; }
    public IList<SelectListItem> AvailablePhoneCountries { get; set; } = new List<SelectListItem>();

    [DataType(DataType.Password)]
    [NoTrim]
    [NopResourceDisplayName("Account.Login.Fields.Password")]
    public string Password { get; set; }

    [NopResourceDisplayName("Account.Login.Fields.RememberMe")]
    public bool RememberMe { get; set; }

    public bool DisplayCaptcha { get; set; }
}