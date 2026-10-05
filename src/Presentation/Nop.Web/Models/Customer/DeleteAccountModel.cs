using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Customer;

public partial record DeleteAccountModel : BaseNopModel
{
    [DataType(DataType.Password)]
    [NoTrim]
    [NopResourceDisplayName("Account.DeleteAccount.Password")]
    public string Password { get; set; }
}