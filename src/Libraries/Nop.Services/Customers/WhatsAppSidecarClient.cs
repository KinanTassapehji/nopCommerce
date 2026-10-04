using System.Net.Http.Json;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Customers;

/// <summary>
/// One WhatsApp number linked to the sidecar
/// </summary>
/// <param name="Id">Sidecar's id of the number</param>
/// <param name="State">starting, qr, authenticated, ready or disconnected</param>
/// <param name="Qr">QR code to scan (a data: URL), while the number waits to be linked</param>
/// <param name="Phone">The linked number (digits only), once ready</param>
public partial record WhatsAppAccount(string Id, string State, string Qr, string Phone);

/// <summary>
/// HTTP client of the WhatsApp sidecar (deploy/whatsapp-sidecar): a Node service that keeps
/// WhatsApp Web logged in on the store's own number and sends messages from it. It listens on
/// the server's loopback only, so it has no authentication of its own
/// </summary>
public partial class WhatsAppSidecarClient
{
    #region Fields

    protected readonly HttpClient _httpClient;
    protected readonly PhoneVerificationSettings _phoneVerificationSettings;

    #endregion

    #region Ctor

    public WhatsAppSidecarClient(HttpClient httpClient,
        PhoneVerificationSettings phoneVerificationSettings)
    {
        _httpClient = httpClient;
        _phoneVerificationSettings = phoneVerificationSettings;

        //finding the number on WhatsApp and sending takes a few seconds on a busy server
        httpClient.Timeout = TimeSpan.FromSeconds(45);
    }

    #endregion

    #region Utilities

    protected virtual Uri GetUrl(string path)
    {
        return new Uri(new Uri(_phoneVerificationSettings.WhatsAppSidecarUrl), path);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get the linked numbers
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the numbers; null when the sidecar does not answer
    /// </returns>
    public virtual async Task<IList<WhatsAppAccount>> GetAccountsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<WhatsAppAccount>>(GetUrl("/accounts")) ?? new List<WhatsAppAccount>();
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or UriFormatException)
        {
            return null;
        }
    }

    /// <summary>
    /// Add a number: its QR code then shows in <see cref="GetAccountsAsync"/>
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task AddAccountAsync()
    {
        (await _httpClient.PostAsync(GetUrl("/accounts"), null)).EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Log a number out and forget it
    /// </summary>
    /// <param name="id">Sidecar's id of the number</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task LogoutAccountAsync(string id)
    {
        (await _httpClient.PostAsync(GetUrl($"/accounts/{Uri.EscapeDataString(id)}/logout"), null)).EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Get whether a number has WhatsApp
    /// </summary>
    /// <param name="accountId">Sidecar's id of the number to ask through</param>
    /// <param name="phone">Number, E.164</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains whether it has; null when the sidecar could not tell
    /// </returns>
    public virtual async Task<bool?> IsOnWhatsAppAsync(string accountId, string phone)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(GetUrl("/check"), new { accountId, phone = phone.TrimStart('+') });
            var result = await response.Content.ReadFromJsonAsync<CheckResult>();

            return result?.Ok == true ? result.OnWhatsApp : null;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or UriFormatException or System.Text.Json.JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Send a text message
    /// </summary>
    /// <param name="accountId">Sidecar's id of the number to send from</param>
    /// <param name="phone">Recipient, E.164 (+963933123456)</param>
    /// <param name="message">Text</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains null when sent, otherwise the sidecar's error (not_on_whatsapp, not_ready...)
    /// </returns>
    public virtual async Task<string> SendAsync(string accountId, string phone, string message)
    {
        try
        {
            //the sidecar takes the number as digits only
            var response = await _httpClient.PostAsJsonAsync(GetUrl("/send"), new { accountId, phone = phone.TrimStart('+'), message });
            var result = await response.Content.ReadFromJsonAsync<SendResult>();

            return result?.Ok == true ? null : result?.Error ?? $"HTTP {(int)response.StatusCode}";
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or UriFormatException or System.Text.Json.JsonException)
        {
            return exception.Message;
        }
    }

    #endregion

    #region Nested classes

    protected partial record SendResult(bool Ok, string Error);

    protected partial record CheckResult(bool Ok, bool OnWhatsApp);

    #endregion
}