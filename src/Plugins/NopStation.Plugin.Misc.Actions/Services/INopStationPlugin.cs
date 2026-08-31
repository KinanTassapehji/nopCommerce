using System.Collections.Generic;
using Nop.Services.Plugins;

namespace NopStation.Plugin.Misc.Core.Services;

public interface INopStationPlugin : IPlugin
{
	IList<string> PluginResourcePrefixes => null;

	bool DeleteObsoletedPluginResources => false;

	IDictionary<string, string> GetPluginResources();
}
