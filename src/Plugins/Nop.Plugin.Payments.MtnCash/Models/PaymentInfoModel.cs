using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.MtnCash.Models;

public record PaymentInfoModel : BaseNopModel
{
    public string Phone { get; set; }
}