using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class StartupEventHostedService : IHostedService
{
	private readonly ILanguageService _languageService;

	private readonly INopStationPluginManager _nopStationPluginManager;

	private readonly ILocalizationService _localizationService;

	private readonly IRepository<LocaleStringResource> _lsrRepository;

	public StartupEventHostedService(ILanguageService languageService, INopStationPluginManager nopStationPluginManager, ILocalizationService localizationService, IRepository<LocaleStringResource> lsrRepository)
	{
		_languageService = languageService;
		_nopStationPluginManager = nopStationPluginManager;
		_localizationService = localizationService;
		_lsrRepository = lsrRepository;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		IList<Language> languages = await _languageService.GetAllLanguagesAsync();
		IList<INopStationPlugin> plugins = await _nopStationPluginManager.LoadNopStationPluginsAsync();
		List<LocaleStringResource> resourcesToDelete = new List<LocaleStringResource>();
		foreach (INopStationPlugin item in plugins.Where((INopStationPlugin p) => p.DeleteObsoletedPluginResources))
		{
			if (item.PluginResourcePrefixes == null || item.PluginResourcePrefixes.Count == 0)
			{
				continue;
			}
			Dictionary<string, string> pluginResoures = item.GetPluginResources().ToDictionary(StringComparer.OrdinalIgnoreCase);
			foreach (string pluginResourcePrefix in item.PluginResourcePrefixes)
			{
				foreach (LocaleStringResource item2 in await GetLocaleResourcesAsync(pluginResourcePrefix))
				{
					if (!pluginResoures.ContainsKey(item2.ResourceName))
					{
						resourcesToDelete.Add(item2);
					}
				}
			}
		}
		if (resourcesToDelete.Count > 0)
		{
			await _lsrRepository.DeleteAsync(resourcesToDelete);
		}
		IEnumerable<KeyValuePair<string, string>> pluginResources = plugins.SelectMany((INopStationPlugin x) => x.GetPluginResources());
		foreach (Language language in languages)
		{
			Dictionary<string, KeyValuePair<int, string>> dictionary = await _localizationService.GetAllResourceValuesAsync(language.Id, null);
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> item3 in pluginResources)
			{
				if (!dictionary.ContainsKey(item3.Key.ToLowerInvariant()))
				{
					dictionary2[item3.Key] = item3.Value;
				}
			}
			if (dictionary2.Count != 0)
			{
				await _localizationService.AddOrUpdateLocaleResourceAsync(dictionary2, language.Id);
			}
		}
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	protected virtual async Task<IList<LocaleStringResource>> GetLocaleResourcesAsync(string resourceNamePrefix, int? languageId = null)
	{
		return await _lsrRepository.Table.Where((LocaleStringResource locale) => (!((int?)languageId).HasValue || locale.LanguageId == ((int?)languageId).Value) && !string.IsNullOrEmpty(locale.ResourceName) && locale.ResourceName.StartsWith(resourceNamePrefix, StringComparison.InvariantCultureIgnoreCase)).ToListAsync();
	}
}
