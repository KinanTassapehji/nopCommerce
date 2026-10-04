using Nop.Services.Customers;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Settings;

/// <summary>
/// Phone verification: the switch, and the store's WhatsApp numbers that send the codes
/// </summary>
public partial record PhoneVerificationSettingsModel : BaseNopModel
{
    [NopResourceDisplayName("Admin.Configuration.Settings.PhoneVerification.Enabled")]
    public bool Enabled { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.PhoneVerification.WhatsAppSidecarUrl")]
    public string WhatsAppSidecarUrl { get; set; }

    /// <summary>
    /// Linked numbers; null when the sidecar does not answer
    /// </summary>
    public IList<WhatsAppAccount> Accounts { get; set; } = new List<WhatsAppAccount>();
}