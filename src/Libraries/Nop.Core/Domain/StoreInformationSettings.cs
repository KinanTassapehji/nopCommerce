using Nop.Core.Configuration;

namespace Nop.Core.Domain;

/// <summary>
/// Store information settings
/// </summary>
public partial class StoreInformationSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether "powered by nopCommerce" text should be displayed.
    /// Please find more info at https://www.nopcommerce.com/nopcommerce-copyright-removal-key
    /// </summary>
    public bool HidePoweredByNopCommerce { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether store is closed
    /// </summary>
    public bool StoreClosed { get; set; }

    /// <summary>
    /// Gets or sets a picture identifier of the logo. If 0, then the default one will be used
    /// </summary>
    public int LogoPictureId { get; set; }

    /// <summary>
    /// Gets or sets a default store theme
    /// </summary>
    public string DefaultStoreTheme { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether customers are allowed to select a theme
    /// </summary>
    public bool AllowCustomerToSelectTheme { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether we should display warnings about the new EU cookie law
    /// </summary>
    public bool DisplayEuCookieLawWarning { get; set; }

    /// <summary>
    /// Gets or sets a value of Facebook page URL of the site
    /// </summary>
    public string FacebookLink { get; set; }

    /// <summary>
    /// Gets or sets a value of Twitter page URL of the site
    /// </summary>
    public string TwitterLink { get; set; }

    /// <summary>
    /// Gets or sets a value of YouTube channel URL of the site
    /// </summary>
    public string YoutubeLink { get; set; }

    /// <summary>
    /// Gets or sets a value of Instagram account URL of the site
    /// </summary>
    public string InstagramLink { get; set; }

    /// <summary>
    /// Gets or sets a value of WhatsApp chat URL of the site (e.g. https://wa.me/9665xxxxxxxx)
    /// </summary>
    public string WhatsAppLink { get; set; }

    /// <summary>
    /// Gets or sets the phone number customers call, shown on the contact page
    /// </summary>
    public string ContactPhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the email address customers write to, shown on the contact page
    /// </summary>
    public string ContactEmail { get; set; }

    /// <summary>
    /// Gets or sets the store's Google Play listing, opened by "Rate us"
    /// </summary>
    public string GooglePlayAppLink { get; set; }

    /// <summary>
    /// Gets or sets the store's App Store listing, opened by "Rate us" on iPhone and iPad
    /// </summary>
    public string AppStoreAppLink { get; set; }
}