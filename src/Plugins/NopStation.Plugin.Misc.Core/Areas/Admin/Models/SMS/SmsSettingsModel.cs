using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;

public record SmsSettingsModel : BaseNopModel, ISettingsModel
{
	[NopResourceDisplayName("Admin.NopStation.Core.SmsSettings.Fields.MaxSendTries")]
	public int MaxSendTries { get; set; }

	public bool MaxSendTries_OverrideForStore { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsSettings.Fields.MaxMessagesPerBatch")]
	public int MaxMessagesPerBatch { get; set; }

	public bool MaxMessagesPerBatch_OverrideForStore { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsSettings.Fields.DelayBetweenMessagesMs")]
	public int DelayBetweenMessagesMs { get; set; }

	public bool DelayBetweenMessagesMs_OverrideForStore { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsSettings.Fields.EnableDetailedLogging")]
	public bool EnableDetailedLogging { get; set; }

	public bool EnableDetailedLogging_OverrideForStore { get; set; }

	public int ActiveStoreScopeConfiguration { get; set; }
}
