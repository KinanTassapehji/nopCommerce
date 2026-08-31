using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.CustomerReminders.Settings;

public class CustomerRemindersSettings : ISettings
{
	public bool IsEnabled { get; set; } = true;

	public bool IsExcludeGuests { get; set; } = true;
}
