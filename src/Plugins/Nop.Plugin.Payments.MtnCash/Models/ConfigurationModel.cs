using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.MtnCash.Models;

public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Payments.MtnCash.Fields.AdditionalFeePercentage")]
    public bool AdditionalFeePercentage { get; set; }

    [NopResourceDisplayName("Plugins.Payments.MtnCash.Fields.AdditionalFee")]
    public decimal AdditionalFee { get; set; } = 0;

}