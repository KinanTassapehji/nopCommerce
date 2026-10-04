using Nop.Core.Configuration;

namespace Nop.Core.Domain.Customers;

/// <summary>
/// Confirming a customer's phone number (their login) with a code sent over WhatsApp
/// </summary>
public partial class PhoneVerificationSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether a new account, a changed phone number and a password
    /// reset by phone need a code. Off until the store's WhatsApp number is linked
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the address of the WhatsApp sidecar (deploy/whatsapp-sidecar), e.g. http://127.0.0.1:3210.
    /// Empty: codes are written to the system log instead of sent (testing without WhatsApp)
    /// </summary>
    public string WhatsAppSidecarUrl { get; set; }
}