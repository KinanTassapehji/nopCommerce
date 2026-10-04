using Nop.Services.Customers;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Customer;

/// <summary>
/// The page where a code sent to the customer's phone is entered
/// </summary>
public partial record VerifyPhoneModel : BaseNopModel
{
    public Guid Guid { get; set; }

    /// <summary>
    /// Number the code went to, formatted for reading
    /// </summary>
    public string Phone { get; set; }

    public PhoneVerificationPurpose Purpose { get; set; }

    [NopResourceDisplayName("Account.PhoneVerification.Code")]
    public string Code { get; set; }

    public string ReturnUrl { get; set; }
}