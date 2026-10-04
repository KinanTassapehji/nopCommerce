using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;

namespace Nop.Services.Customers;

/// <summary>
/// Confirms that a customer owns a phone number by sending a code to it over WhatsApp
/// </summary>
public partial class PhoneVerificationService : IPhoneVerificationService
{
    #region Constants

    protected const string CODE_ATTRIBUTE = "PhoneVerificationCode";
    protected const string PENDING_ATTRIBUTE = "PhoneVerificationPending";

    protected static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    protected static readonly TimeSpan ResendDelay = TimeSpan.FromSeconds(60);
    protected const int MAX_ATTEMPTS = 5;
    protected const int MAX_SENDS_PER_NUMBER_PER_HOUR = 3;
    //every code is a message from the store's own WhatsApp number, and a burst of them to
    //strangers is what gets a number banned
    protected const int MAX_SENDS_PER_VISITOR_PER_HOUR = 10;

    #endregion

    #region Fields

    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILogger _logger;
    protected readonly IMemoryCache _memoryCache;
    protected readonly IWebHelper _webHelper;
    protected readonly PhoneVerificationSettings _phoneVerificationSettings;
    protected readonly WhatsAppSidecarClient _whatsAppSidecarClient;

    #endregion

    #region Ctor

    public PhoneVerificationService(IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        ILogger logger,
        IMemoryCache memoryCache,
        IWebHelper webHelper,
        PhoneVerificationSettings phoneVerificationSettings,
        WhatsAppSidecarClient whatsAppSidecarClient)
    {
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
        _logger = logger;
        _memoryCache = memoryCache;
        _webHelper = webHelper;
        _phoneVerificationSettings = phoneVerificationSettings;
        _whatsAppSidecarClient = whatsAppSidecarClient;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Hash a code; salted with the customer, so equal codes of two customers do not match
    /// </summary>
    /// <param name="salt">Customer GUID</param>
    /// <param name="code">Code</param>
    /// <returns>Hash</returns>
    public static string HashCode(Guid salt, string code)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{code}")));
    }

    /// <summary>
    /// Check an entered code against the one sent
    /// </summary>
    /// <param name="sent">Code sent; null when none</param>
    /// <param name="salt">Customer GUID</param>
    /// <param name="entered">Code as entered</param>
    /// <param name="utcNow">Current time</param>
    /// <returns>Result</returns>
    public static PhoneCodeCheckResult Check(PhoneCode sent, Guid salt, string entered, DateTime utcNow)
    {
        if (string.IsNullOrEmpty(sent?.Hash))
            return PhoneCodeCheckResult.NoCode;
        if (sent.Attempts >= MAX_ATTEMPTS)
            return PhoneCodeCheckResult.TooManyAttempts;
        if (sent.ExpiresUtc <= utcNow)
            return PhoneCodeCheckResult.Expired;

        //customers paste "123 456" or type Arabic-Indic digits
        var digits = new string((entered ?? string.Empty).Where(char.IsDigit).Select(digit => (char)('0' + (int)char.GetNumericValue(digit))).ToArray());
        var match = CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(HashCode(salt, digits)), Encoding.ASCII.GetBytes(sent.Hash));

        return match ? PhoneCodeCheckResult.Valid : PhoneCodeCheckResult.Wrong;
    }

    /// <summary>
    /// Deliver a code over WhatsApp
    /// </summary>
    /// <param name="phone">Number, E.164</param>
    /// <param name="code">Code</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains null when delivered, otherwise the error
    /// </returns>
    protected virtual async Task<string> DeliverAsync(string phone, string code)
    {
        //no sidecar (local testing): the code goes to the system log
        if (string.IsNullOrEmpty(_phoneVerificationSettings.WhatsAppSidecarUrl))
        {
            await _logger.InformationAsync($"Phone verification: code {code} for {phone} - no WhatsApp sidecar set, nothing was sent");
            return null;
        }

        var account = await GetSendingAccountAsync();
        if (account == null)
            return "no WhatsApp number is connected";

        var message = string.Format(await _localizationService.GetResourceAsync("Account.PhoneVerification.Message"), code);

        return await _whatsAppSidecarClient.SendAsync(account.Id, phone, message);
    }

    /// <summary>
    /// Pick the store number to send from; with more than one linked, the codes are spread over them
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a connected number; null when none is
    /// </returns>
    protected virtual async Task<WhatsAppAccount> GetSendingAccountAsync()
    {
        var ready = (await _whatsAppSidecarClient.GetAccountsAsync())?.Where(account => account.State == "ready").ToList();

        return ready is { Count: > 0 } ? ready[Random.Shared.Next(ready.Count)] : null;
    }

    protected virtual async Task SaveCodeAsync(Customer customer, PhoneCode code)
    {
        await _genericAttributeService.SaveAttributeAsync(customer, CODE_ATTRIBUTE, JsonSerializer.Serialize(code));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get the code last sent to a customer
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the code; null when none was sent
    /// </returns>
    public virtual async Task<PhoneCode> GetCodeAsync(Customer customer)
    {
        var json = await _genericAttributeService.GetAttributeAsync<string>(customer, CODE_ATTRIBUTE);

        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<PhoneCode>(json);
    }

    /// <summary>
    /// Get whether a number can receive a code
    /// </summary>
    /// <param name="phone">Number, E.164</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains false only when WhatsApp says the number is not on it
    /// </returns>
    public virtual async Task<bool> CanReceiveCodeAsync(string phone)
    {
        if (string.IsNullOrEmpty(_phoneVerificationSettings.WhatsAppSidecarUrl))
            return true;

        //no connected number, or no answer: let the send itself fail and say so
        var account = await GetSendingAccountAsync();

        return account == null || await _whatsAppSidecarClient.IsOnWhatsAppAsync(account.Id, phone) != false;
    }

    /// <summary>
    /// Send a new code, replacing the previous one
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="phone">Number to send to, E.164</param>
    /// <param name="purpose">What the code confirms</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains how it went
    /// </returns>
    public virtual async Task<PhoneCodeSendResult> SendCodeAsync(Customer customer, string phone, PhoneVerificationPurpose purpose)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var now = DateTime.UtcNow;
        var code = await GetCodeAsync(customer) ?? new PhoneCode();
        code.SentUtc = code.SentUtc.Where(sent => sent > now.AddHours(-1)).ToList();

        //the wait is for asking the same code again; a code for something else (a password reset
        //right after registering) goes out, within the hourly limits
        var sameCode = !string.IsNullOrEmpty(code.Hash) && code.Purpose == purpose && code.Phone == phone;
        if (sameCode && code.SentUtc.Any(sent => sent > now - ResendDelay))
            return PhoneCodeSendResult.TooSoon;
        if (code.SentUtc.Count >= MAX_SENDS_PER_NUMBER_PER_HOUR)
            return PhoneCodeSendResult.TooMany;

        //ponytail: per-process counter, so each server instance allows its own 10; a shared store if the site is ever load-balanced
        var visitor = _memoryCache.GetOrCreate($"{CODE_ATTRIBUTE}.{_webHelper.GetCurrentIpAddress()}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return new StrongBox<int>();
        });
        if (visitor.Value >= MAX_SENDS_PER_VISITOR_PER_HOUR)
            return PhoneCodeSendResult.TooMany;

        var digits = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var error = await DeliverAsync(phone, digits);
        if (error == "not_on_whatsapp")
            return PhoneCodeSendResult.NotOnWhatsApp;
        if (error != null)
        {
            await _logger.ErrorAsync($"Phone verification: the code for {phone} was not sent over WhatsApp: {error}");
            return PhoneCodeSendResult.Failed;
        }

        Interlocked.Increment(ref visitor.Value);
        code.Hash = HashCode(customer.CustomerGuid, digits);
        code.Phone = phone;
        code.Purpose = purpose;
        code.ExpiresUtc = now + CodeLifetime;
        code.Attempts = 0;
        code.SentUtc.Add(now);
        await SaveCodeAsync(customer, code);

        return PhoneCodeSendResult.Sent;
    }

    /// <summary>
    /// Check an entered code; a valid code is used up
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="code">Code as entered</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains how it went
    /// </returns>
    public virtual async Task<PhoneCodeCheckResult> CheckCodeAsync(Customer customer, string code)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var sent = await GetCodeAsync(customer);
        var result = Check(sent, customer.CustomerGuid, code, DateTime.UtcNow);

        if (result == PhoneCodeCheckResult.Wrong)
        {
            sent.Attempts++;
            await SaveCodeAsync(customer, sent);
        }
        else if (result == PhoneCodeCheckResult.Valid)
        {
            //used up; the send history stays for the rate limit
            sent.Hash = null;
            await SaveCodeAsync(customer, sent);
        }

        return result;
    }

    /// <summary>
    /// Get whether a new account still waits for its number to be confirmed
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains true while it waits
    /// </returns>
    public virtual async Task<bool> IsActivationPendingAsync(Customer customer)
    {
        return await _genericAttributeService.GetAttributeAsync<bool>(customer, PENDING_ATTRIBUTE);
    }

    /// <summary>
    /// Set whether a new account waits for its number to be confirmed
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="pending">True while it waits</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task SetActivationPendingAsync(Customer customer, bool pending)
    {
        //false removes the attribute
        await _genericAttributeService.SaveAttributeAsync(customer, PENDING_ATTRIBUTE, pending ? true : (bool?)null);
    }

    #endregion
}