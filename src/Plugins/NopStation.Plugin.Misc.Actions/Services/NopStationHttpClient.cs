using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Services.Plugins;

namespace NopStation.Plugin.Misc.Core.Services;

public class NopStationHttpClient
{
	private const string INSTALLATION_URL = "https://www.nop-station.com/plugin-installation/";

	private const string UNINSTALLATION_URL = "https://www.nop-station.com/plugin-uninstallation/";

	private readonly HttpClient _httpClient;

	private readonly IWebHelper _webHelper;

	public NopStationHttpClient(HttpClient client, IWebHelper webHelper)
	{
		client.Timeout = TimeSpan.FromSeconds(10L);
		client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "nopCommerce-4.90");
		client.DefaultRequestHeaders.Add("X-Version", "1");
		_httpClient = client;
		_webHelper = webHelper;
	}

	private async Task RequestNopStationAsync(PluginDescriptor plugin, string url)
	{
		try
		{
			string storeLocation = _webHelper.GetStoreLocation();
			if (!string.IsNullOrEmpty(storeLocation) && !storeLocation.Contains("localhost") && !storeLocation.Contains("127.0.0.1") && !storeLocation.Contains(".local"))
			{
				StringContent content = new StringContent($"url={storeLocation}&product={plugin.SystemName}&version={plugin.Version}", Encoding.UTF8, MimeTypes.ApplicationXWwwFormUrlencoded);
				HttpResponseMessage obj = await _httpClient.PostAsync(url, content);
				obj.EnsureSuccessStatusCode();
				await obj.Content.ReadAsStringAsync();
			}
		}
		catch
		{
		}
	}

	public async Task OnInstallPluginAsync(PluginDescriptor plugin)
	{
		await RequestNopStationAsync(plugin, "https://www.nop-station.com/plugin-installation/");
	}

	public async Task OnUninstallPluginAsync(PluginDescriptor plugin)
	{
		await RequestNopStationAsync(plugin, "https://www.nop-station.com/plugin-uninstallation/");
	}
}
