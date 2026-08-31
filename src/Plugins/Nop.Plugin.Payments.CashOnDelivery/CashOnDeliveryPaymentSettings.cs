using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.CashOnDelivery;

public class CashOnDeliveryPaymentSettings : ISettings
{
	public decimal AdditionalFee { get; set; }

	public bool AdditionalFeePercentage { get; set; }

	public string DescriptionText { get; set; }

	public bool ShippableProductRequired { get; set; }

	public bool SkipPaymentInfo { get; set; }
}
