using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace NopStation.Plugin.Misc.Core.Services;

public class NopStationPluginManager : PluginManager<INopStationPlugin>, INopStationPluginManager
{
	private readonly ILocalizationService _localizationService;

	public NopStationPluginManager(IPluginService pluginService, ILocalizationService localizationService, ICustomerService customerService)
		: base(customerService, pluginService)
	{
		_localizationService = localizationService;
	}

	public virtual async Task<IList<INopStationPlugin>> LoadNopStationPluginsAsync(Customer customer = null, string pluginSystemName = "", int storeId = 0)
	{
		return (await LoadAllPluginsAsync(customer, storeId)).Where((INopStationPlugin plugin) => string.IsNullOrWhiteSpace(pluginSystemName) || plugin.PluginDescriptor.SystemName.Equals(pluginSystemName)).ToList();
	}

	public virtual async Task<IPagedList<(string Key, string Value)>> LoadPluginStringResourcesAsync(string pluginSystemName = "", string keyword = "", int languageId = 0, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		List<KeyValuePair<string, string>> list = (from x in (await LoadNopStationPluginsAsync(null, pluginSystemName, storeId)).SelectMany((INopStationPlugin x) => from y in x.GetPluginResources()
				where string.IsNullOrWhiteSpace(keyword) || y.Key.Contains(keyword, StringComparison.InvariantCultureIgnoreCase)
				select y)
			orderby x.Key
			select x).ToList();
		int total = list.Count;
		list = list.Skip(pageIndex * pageSize).Take(pageSize).ToList();
		List<(string, string)> pagedResources = new List<(string, string)>();
		foreach (KeyValuePair<string, string> item in list)
		{
			string resource = await _localizationService.GetResourceAsync(item.Key, languageId, logIfNotFound: false, "", returnEmptyIfNotFound: true);
			if (string.IsNullOrEmpty(resource))
			{
				resource = item.Value;
				await _localizationService.InsertLocaleStringResourceAsync(new LocaleStringResource
				{
					ResourceName = item.Key,
					LanguageId = languageId,
					ResourceValue = resource
				});
			}
			pagedResources.Add((item.Key, resource));
		}
		return new PagedList<(string, string)>(pagedResources, pageIndex, pageSize, total);
	}
}
