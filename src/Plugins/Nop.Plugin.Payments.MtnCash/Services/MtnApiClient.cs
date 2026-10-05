using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Plugin.Payments.MtnCash.Interfaces;
using Nop.Services.Configuration;

namespace Nop.Plugin.Payments.MtnCash.Services;
internal class MtnApiClient : IMtnApiClient
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ISettingService _settingsService;
    private const string baseUrl = "https://cashmobile.mtnsyr.com:9000";
    private readonly ILogger<MtnApiClient> _logger;

    public MtnApiClient(
        IHttpClientFactory clientFactory,
        ISettingService settings,
        ILogger<MtnApiClient> logger)
    {
        _clientFactory = clientFactory;
        _settingsService = settings;
        _logger = logger;
    }

    public async Task<string> PostAsync(string requestName, object body)
    {
        var settings = await _settingsService.LoadSettingAsync<MtnCashPaymentSettings>();
        var json = JsonConvert.SerializeObject(body);
        var signature = await SignData(json);

        HttpClient client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);

        AddHeaders(client, settings.TerminalId, requestName, signature);

        using StringContent content =
            new(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response =
            await client.PostAsync($"/{requestName}", content);

        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            _logger.LogError($"MTN_CASH Error: {response.StatusCode}");

        return responseBody;
    }

    public async Task<string> GetAsync(string requestName, object query)
    {
        var settings = await _settingsService.LoadSettingAsync<MtnCashPaymentSettings>();

        string json = JsonConvert.SerializeObject(query);
        string signature = await SignData(json);

        HttpClient client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl);

        AddHeaders(client, settings.TerminalId, requestName, signature);

        HttpResponseMessage response =
            await client.GetAsync($"/{requestName}");

        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            _logger.LogError($"MTN_CASH Error: {response.StatusCode}");

        return responseBody;
    }
    public async Task ActivateTerminalAsync()
    {
        var settings = await _settingsService.LoadSettingAsync<MtnCashPaymentSettings>();

        var publicKeyBytes = Convert.FromBase64String(settings.Base64PublicKey);
        var normalizedPublicKey = "";
        var requestBody = new
        {
            Key = settings.Base64PublicKey,
            Secret = settings.TerminalActivationCode,
            Serial = settings.TerminalNumer,
        };

        string response = await PostAsync(
            "pos_web/pos/activate",
            requestBody
        );

        JObject json = JObject.Parse(response);

        if ((int)json["Errno"] != 0)
        {
            throw json["Errno"]!.Value<int>() switch
            {
                301 => new Exception("The terminal is already activated."),
                300 => new Exception("Incorrect POS number or activation code."),
                302 => new Exception("POS terminal is blocked."),
                303 => new Exception("POS terminal is deactivated."),
                304 => new Exception("Activation failed, use new activation code."),
                _ => new Exception(json["Error"]!.ToString())
            };
        }
    }


    private static void AddHeaders(
        HttpClient client,
        string terminalId,
        string requestName,
        string signature)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Subject", terminalId);
        client.DefaultRequestHeaders.Add("Request-Name", requestName);
        client.DefaultRequestHeaders.Add("X-Signature", signature);
        client.DefaultRequestHeaders.Add(
            "Accept-Language",
            CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
    }

    private async Task<string> SignData(string data)
    {
        var settings = await _settingsService.LoadSettingAsync<MtnCashPaymentSettings>();
        byte[] privateKeyBytes = Convert.FromBase64String(settings.Base64PrivateKey);

        using RSA rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

        byte[] signatureBytes =
            rsa.SignData(
                Encoding.UTF8.GetBytes(data),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signatureBytes);
    }
    private async Task<bool> VerifySignature(string data, string signatureBase64)
    {
        var settings = await _settingsService.LoadSettingAsync<MtnCashPaymentSettings>();
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] signatureBytes = Convert.FromBase64String(signatureBase64);
        byte[] publicKeyBytes = Convert.FromBase64String(settings.Base64PublicKey);

        using RSA rsa = RSA.Create();
        // Import the public key in X.509 format
        rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

        // Verify the signature using SHA256 and PKCS#1 v1.5 padding
        return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
