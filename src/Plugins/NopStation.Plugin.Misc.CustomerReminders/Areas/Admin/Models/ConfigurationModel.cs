using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
	public int ActiveStoreScopeConfiguration { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Configuration.Fields.Enabled")]
	public bool Enabled { get; set; }

	public bool Enabled_OverrideForStore { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.Configuration.Fields.ExcludeGuests")]
	public bool ExcludeGuests { get; set; }

	public bool ExcludeGuests_OverrideForStore { get; set; }
}
