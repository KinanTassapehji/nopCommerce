using System.Net.Security;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nop.Plugin.Payments.Syriatel.Interfaces;
using Nop.Services.Configuration;

namespace Nop.Plugin.Payments.Syriatel.Services;

internal class SyriatelApiClient : ISyriatelApiClient
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ISettingService _settingsService;
    private readonly ILogger<SyriatelApiClient> _logger;

    public SyriatelApiClient(
        IHttpClientFactory clientFactory,
        ISettingService settingsService,
        ILogger<SyriatelApiClient> logger)
    {
        _clientFactory = clientFactory;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task<string> PostAsync(string endpoint, object body)
    {
        var settings = await _settingsService.LoadSettingAsync<SyriatelPaymentSettings>();
        var json = JsonConvert.SerializeObject(body);

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        using var client = new HttpClient(handler);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

        var url = $"{settings.BaseUrl.TrimEnd('/')}/{endpoint}";

        using StringContent content =
            new(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response =
            await client.PostAsync(url, content);

        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            _logger.LogError("Syriatel API error: {StatusCode} - {Body}", response.StatusCode, responseBody);

        return responseBody;
    }
}
