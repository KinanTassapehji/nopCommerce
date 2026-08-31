using System.Collections.Generic;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class AdminMenuEvent
{
	public IList<NopStationAdminMenuItem> PluginChildNodes { get; set; }

	public IList<NopStationAdminMenuItem> ThemeChildNodes { get; set; }

	public IList<NopStationAdminMenuItem> CoreChildNodes { get; set; }

	public IList<NopStationAdminMenuItem> RootChildNodes { get; set; }

	public AdminMenuEvent()
	{
		PluginChildNodes = new List<NopStationAdminMenuItem>();
		ThemeChildNodes = new List<NopStationAdminMenuItem>();
		CoreChildNodes = new List<NopStationAdminMenuItem>();
		RootChildNodes = new List<NopStationAdminMenuItem>();
	}
}
