using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.Core.Domains.SMS;

public class SmsSettings : ISettings
{
	public int MaxSendTries { get; set; } = 3;

	public int MaxMessagesPerBatch { get; set; } = 100;

	public int DelayBetweenMessagesMs { get; set; } = 100;

	public bool EnableDetailedLogging { get; set; }
}
