using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Syriatel.Models;

public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Payments.Syriatel.Fields.BaseUrl")]
    public string BaseUrl { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Syriatel.Fields.Username")]
    public string Username { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Syriatel.Fields.Password")]
    public string Password { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Syriatel.Fields.MerchantMsisdn")]
    public string MerchantMsisdn { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Syriatel.Fields.AdditionalFeePercentage")]
    public bool AdditionalFeePercentage { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Syriatel.Fields.AdditionalFee")]
    public decimal AdditionalFee { get; set; } = 0;

}
