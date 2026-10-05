using Newtonsoft.Json.Linq;
using Nop.Plugin.Payments.Syriatel;
using Nop.Plugin.Payments.Syriatel.Interfaces;
using Nop.Services.Configuration;

namespace Nop.Plugin.Payments.Syriatel.Services;

internal class SyriatelTokenProvider : ISyriatelTokenProvider
{
    private readonly ISyriatelApiClient _apiClient;
    private readonly ISettingService _settingService;

    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static SyriatelTokenState _state;

    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan SafetyMargin = TimeSpan.FromSeconds(30);

    public SyriatelTokenProvider(
        ISyriatelApiClient apiClient,
        ISettingService settingService)
    {
        _apiClient = apiClient;
        _settingService = settingService;
    }

    public async Task<string> GetTokenAsync()
    {
        // Fast path — token still valid
        if (_state != null && !IsExpired(_state))
            return _state.Token;

        await _lock.WaitAsync();
        try
        {
            // Double-check after lock
            if (_state != null && !IsExpired(_state))
                return _state.Token;

            var settings = await _settingService.LoadSettingAsync<SyriatelPaymentSettings>();

            var requestBody = new
            {
                username = settings.Username,
                password = settings.Password
            };

            var response = await _apiClient.PostAsync("getToken", requestBody);
            var json = JObject.Parse(response);

            var errorCode = json.Value<int>("errorCode");
            if (errorCode != 0)
            {
                var errorDesc = json.Value<string>("errorDesc") ?? "Unknown error";
                throw new Exception($"Syriatel getToken failed ({errorCode}): {errorDesc}");
            }

            var token = json.Value<string>("token");
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Syriatel token missing");

            _state = new SyriatelTokenState
            {
                Token = token,
                GeneratedAtUtc = DateTime.UtcNow
            };

            return token;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Invalidate()
    {
        _state = null;
    }

    private static bool IsExpired(SyriatelTokenState state)
    {
        return DateTime.UtcNow >= state.GeneratedAtUtc
            .Add(TokenLifetime)
            .Subtract(SafetyMargin);
    }
}
