namespace Nop.Services.Customers;

/// <summary>
/// What a phone code confirms
/// </summary>
public enum PhoneVerificationPurpose
{
    /// <summary>
    /// A new account: it stays inactive until its number is confirmed
    /// </summary>
    Activate = 1,

    /// <summary>
    /// A new number on an existing account (the number is the login)
    /// </summary>
    ChangePhone = 2,

    /// <summary>
    /// A forgotten password, recovered by phone
    /// </summary>
    ResetPassword = 3
}

/// <summary>
/// How sending a code went
/// </summary>
public enum PhoneCodeSendResult
{
    Sent,

    /// <summary>
    /// A code went out moments ago
    /// </summary>
    TooSoon,

    /// <summary>
    /// Too many codes for this number or this visitor within the hour
    /// </summary>
    TooMany,

    NotOnWhatsApp,

    /// <summary>
    /// The store's WhatsApp number is not connected, or the send failed (logged)
    /// </summary>
    Failed
}

/// <summary>
/// How checking an entered code went
/// </summary>
public enum PhoneCodeCheckResult
{
    Valid,
    Wrong,
    Expired,
    TooManyAttempts,

    /// <summary>
    /// No code was sent, or it was used already
    /// </summary>
    NoCode
}

/// <summary>
/// The code last sent to a customer, kept (hashed) in a generic attribute
/// </summary>
public partial class PhoneCode
{
    /// <summary>
    /// Hash of the code; null once used
    /// </summary>
    public string Hash { get; set; }

    /// <summary>
    /// Number the code was sent to, E.164
    /// </summary>
    public string Phone { get; set; }

    public PhoneVerificationPurpose Purpose { get; set; }

    public DateTime ExpiresUtc { get; set; }

    /// <summary>
    /// Wrong codes entered so far
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// When codes were sent in the last hour, for the rate limit
    /// </summary>
    public List<DateTime> SentUtc { get; set; } = new();
}