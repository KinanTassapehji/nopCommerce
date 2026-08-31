using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Domains.Marketplace;

namespace NopStation.Plugin.Misc.Core.Services;

public class MarketplaceService : IMarketplaceService
{
	private const string BASE_API_URL = "https://www.stage.nop-station.site/";

	private const string MARKETPLACE_API_URL = "https://www.stage.nop-station.site/api-marketplace/products";

	private const string MARKETPLACE_DOWNLOAD_URL = "https://www.stage.nop-station.site/api-marketplace/download/";

	private const string MARKETPLACE_UPGRADE_REQUEST_URL = "https://www.stage.nop-station.site/api-marketplace/upgrade-request/";

	private const string STORE_URL_HEADER = "X-Store-Url";

	private const string USER_EMAIL_HEADER = "X-User-Email";

	private const int API_TIMEOUT_SECONDS = 30;

	private readonly HttpClient _httpClient;

	private readonly IUploadService _uploadService;

	private readonly IWebHelper _webHelper;

	private readonly IWorkContext _workContext;

	public MarketplaceService(HttpClient httpClient, IUploadService uploadService, IWebHelper webHelper, IWorkContext workContext)
	{
		httpClient.Timeout = TimeSpan.FromSeconds(30L);
		_httpClient = httpClient;
		_uploadService = uploadService;
		_webHelper = webHelper;
		_workContext = workContext;
	}

	private static string BuildQueryString(int categoryId = 0, int paidFilter = 0, string searchText = null, int versionFilter = 0, int page = 1)
	{
		List<string> list = new List<string>();
		if (categoryId > 0)
		{
			list.Add($"categoryId={categoryId}");
		}
		switch (paidFilter)
		{
		case 1:
			list.Add("isPaid=true");
			break;
		case 2:
			list.Add("isPaid=false");
			break;
		}
		if (!string.IsNullOrWhiteSpace(searchText))
		{
			list.Add("searchText=" + Uri.EscapeDataString(searchText));
		}
		if (versionFilter == 1)
		{
			list.Add("nopVersion=" + Uri.EscapeDataString("4.90"));
		}
		if (page > 1)
		{
			list.Add($"pageNumber={page}");
		}
		return string.Join("&", list);
	}

	public virtual async Task<ApiResponse> GetMarketplaceProductsAsync(int categoryId = 0, int paidFilter = 0, string searchText = null, int versionFilter = 0, int page = 1)
	{
		string text = BuildQueryString(categoryId, paidFilter, searchText, versionFilter, page);
		string requestUri = (string.IsNullOrEmpty(text) ? "https://www.stage.nop-station.site/api-marketplace/products" : ("https://www.stage.nop-station.site/api-marketplace/products?" + text));
		string json = await _httpClient.GetStringAsync(requestUri);
		JsonSerializerOptions options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
		return JsonSerializer.Deserialize<ApiResponse>(json, options) ?? new ApiResponse();
	}

	public virtual async Task<IList<string>> DownloadAndInstallPluginAsync(int productId, string systemName)
	{
		if (productId <= 0)
		{
			throw new ArgumentException("Product ID must be greater than zero.", "productId");
		}
		string value = Uri.EscapeDataString("4.90");
		string requestUri = $"{"https://www.stage.nop-station.site/api-marketplace/download/"}{productId}/{value}";
		string fileName = (string.IsNullOrWhiteSpace(systemName) ? $"marketplace-plugin-{productId}.zip" : (systemName + ".zip"));
		if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
		{
			fileName = $"marketplace-plugin-{productId}.zip";
		}
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
		request.Headers.TryAddWithoutValidation("X-Store-Url", _webHelper.GetStoreLocation());
		HttpRequestHeaders headers = request.Headers;
		headers.TryAddWithoutValidation("X-User-Email", (await _workContext.GetCurrentCustomerAsync()).Email ?? string.Empty);
		using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
		response.EnsureSuccessStatusCode();
		using MemoryStream memoryStream = new MemoryStream();
		await (await response.Content.ReadAsStreamAsync()).CopyToAsync(memoryStream);
		memoryStream.Position = 0L;
		FormFile archivefile = new FormFile(memoryStream, 0L, memoryStream.Length, "pluginArchive", fileName)
		{
			Headers = new HeaderDictionary(),
			ContentType = "application/zip"
		};
		return (from d in (await _uploadService.UploadPluginsAndThemesAsync(archivefile)).OfType<PluginDescriptor>()
			select d.SystemName).ToList();
	}

	public virtual async Task SendUpgradeRequestAsync(string systemName, string version)
	{
		if (string.IsNullOrWhiteSpace(systemName))
		{
			throw new ArgumentException("System name must not be empty.", "systemName");
		}
		if (string.IsNullOrWhiteSpace(version))
		{
			throw new ArgumentException("Version must not be empty.", "version");
		}
		string text = Uri.EscapeDataString(systemName);
		string text2 = Uri.EscapeDataString(version);
		string requestUri = "https://www.stage.nop-station.site/api-marketplace/upgrade-request/" + text + "/" + text2;
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
		request.Headers.TryAddWithoutValidation("X-Store-Url", _webHelper.GetStoreLocation());
		HttpRequestHeaders headers = request.Headers;
		headers.TryAddWithoutValidation("X-User-Email", (await _workContext.GetCurrentCustomerAsync()).Email ?? string.Empty);
		using HttpResponseMessage httpResponseMessage = await _httpClient.SendAsync(request);
		httpResponseMessage.EnsureSuccessStatusCode();
	}
}
