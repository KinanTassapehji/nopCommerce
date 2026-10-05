using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Syriatel.Models;

public record PaymentInfoModel : BaseNopModel
{
    public string CustomerMsisdn { get; set; }
}
