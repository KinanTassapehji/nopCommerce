using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Syriatel;

/// <summary>
/// Represents settings of Syriatel payment plugin
/// </summary>
public class SyriatelPaymentSettings : ISettings
{
    public string BaseUrl { get; set; } = SyriatelConsts.DefaultBaseUrl;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string MerchantMsisdn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
    /// </summary>
    public bool AdditionalFeePercentage { get; set; }

    /// <summary>
    /// Gets or sets an additional fee
    /// </summary>
    public decimal AdditionalFee { get; set; }
}
