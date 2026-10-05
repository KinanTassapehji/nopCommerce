using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.MtnCash;

/// <summary>
/// Represents settings of MtnCash payment plugin
/// </summary>
public class MtnCashPaymentSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
    /// </summary>
    public bool AdditionalFeePercentage { get; set; }

    /// <summary>
    /// Gets or sets an additional fee
    /// </summary>
    public decimal AdditionalFee { get; set; }
    public string TerminalId { get; set; }
    public string TerminalNumer { get; set; }
    public string Base64PrivateKey { get; set; } = "";
    public string Base64PublicKey { get; set; } = "";

    /// <summary>
    /// One-time code MTN issues for activating the terminal (pos/activate)
    /// </summary>
    public string TerminalActivationCode { get; set; } = "";
}