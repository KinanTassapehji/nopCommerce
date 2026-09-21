using System.Text.RegularExpressions;
using Nop.Core.Domain;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Media;

namespace Nop.Services.Messages;

/// <summary>
/// Wraps a message template body in the store's identity - logo, brand rule,
/// card and footer - so every notification leaves looking like the storefront
/// instead of like naked template HTML.
///
/// Applied once, in <see cref="WorkflowMessageService.SendNotificationAsync"/>,
/// so the ~45 templates in the database stay untouched: an admin still edits
/// plain content and the chrome comes from here.
/// </summary>
public static partial class EmailLayout
{
    #region Utilities

    /// <summary>
    /// Read a replaced token by key
    /// </summary>
    /// <param name="tokens">Tokens available to the message</param>
    /// <param name="key">Token key</param>
    /// <returns>Token value, or an empty string when the message does not carry it</returns>
    private static string TokenValue(IList<Token> tokens, string key)
    {
        return tokens?.FirstOrDefault(token => token.Key == key)?.Value?.ToString()?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Get an absolute URL of the store logo
    /// </summary>
    /// <param name="storeUrl">Store location, since a queued message has no request to borrow one from</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the logo URL, or an empty string when there is none to show
    /// </returns>
    private static async Task<string> GetLogoUrlAsync(string storeUrl)
    {
        try
        {
            var storeInformationSettings = EngineContext.Current.Resolve<StoreInformationSettings>();
            if (storeInformationSettings.LogoPictureId == 0)
                return string.Empty;

            var pictureService = EngineContext.Current.Resolve<IPictureService>();

            return await pictureService.GetPictureUrlAsync(storeInformationSettings.LogoPictureId,
                showDefaultPicture: false, storeLocation: storeUrl);
        }
        catch
        {
            //a message can be queued outside a request scope, where the picture service is out of reach.
            //the wordmark below covers that, and a missing logo is never worth losing the message over
            return string.Empty;
        }
    }

    /// <summary>
    /// Drop the store name link the stock templates open with - the header above it says the same thing
    /// </summary>
    /// <param name="body">Message body</param>
    /// <param name="storeName">Store name</param>
    /// <returns>Message body</returns>
    private static string TrimLeadingStoreLink(string body, string storeName)
    {
        if (string.IsNullOrEmpty(storeName))
            return body;

        return Regex.Replace(body,
            @"\A\s*(<p>)?\s*<a\s[^>]*>\s*" + Regex.Escape(storeName) + @"\s*</a>\s*(<br\s*/?>\s*)*",
            "$1", RegexOptions.IgnoreCase);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Wrap a token-replaced message body in the store's identity
    /// </summary>
    /// <param name="body">Message body, with tokens already replaced</param>
    /// <param name="tokens">Tokens available to the message, read for the store details</param>
    /// <param name="rtl">Whether the message language reads right to left</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the message body
    /// </returns>
    public static async Task<string> WrapAsync(string body, IList<Token> tokens, bool rtl)
    {
        if (string.IsNullOrWhiteSpace(body))
            return body;

        var storeName = TokenValue(tokens, "Store.Name");
        var storeUrl = TokenValue(tokens, "Store.URL").TrimEnd('/');
        var companyAddress = TokenValue(tokens, "Store.CompanyAddress");
        var companyPhone = TokenValue(tokens, "Store.CompanyPhoneNumber");
        var storeEmail = TokenValue(tokens, "Store.Email");

        var direction = rtl ? "rtl" : "ltr";
        var start = rtl ? "right" : "left";

        var logoUrl = await GetLogoUrlAsync(storeUrl);

        var brand = !string.IsNullOrEmpty(logoUrl)
            ? $"<img src=\"{logoUrl}\" alt=\"{storeName}\" height=\"44\" style=\"display:block;height:44px;width:auto;border:0;\" />"
            : $"<span style=\"font-size:22px;font-weight:700;color:{StoreBrand.PRIMARY};\">{storeName}</span>";

        if (!string.IsNullOrEmpty(storeUrl))
            brand = $"<a href=\"{storeUrl}\" style=\"text-decoration:none;color:{StoreBrand.PRIMARY};\">{brand}</a>";

        //store details, on one line, however many of them the store has filled in
        var details = new[] { companyAddress, companyPhone, storeEmail }
            .Where(line => !string.IsNullOrEmpty(line))
            .ToList();

        var footer = string.Join(" &nbsp;&middot;&nbsp; ", details);
        if (!string.IsNullOrEmpty(storeUrl))
            footer = $"<a href=\"{storeUrl}\" style=\"color:{StoreBrand.PRIMARY};text-decoration:none;\">{storeUrl}</a>"
                + (string.IsNullOrEmpty(footer) ? string.Empty : $"<br />{footer}");

        //template bodies carry their own links, and an inline style cannot reach them.
        //clients that drop the style block just fall back to their default link colour
        var css = "<style>a{color:" + StoreBrand.PRIMARY + ";} p{margin:0 0 12px;} td{vertical-align:top;}</style>";

        return $"""
            {css}
            <table role="presentation" dir="{direction}" width="100%" cellpadding="0" cellspacing="0" border="0" style="margin:0;padding:24px 12px;background-color:{StoreBrand.SURFACE_ALT};font-family:'Segoe UI','Noto Sans Arabic',Tahoma,Arial,sans-serif;">
              <tr>
                <td align="center">
                  <table role="presentation" width="640" cellpadding="0" cellspacing="0" border="0" style="width:100%;max-width:640px;background-color:{StoreBrand.WHITE};border:1px solid {StoreBrand.LINE};border-radius:12px;overflow:hidden;">
                    <tr>
                      <td align="{start}" style="padding:22px 28px 16px;">{brand}</td>
                    </tr>
                    <tr>
                      <td style="height:4px;line-height:4px;font-size:0;background-color:{StoreBrand.PRIMARY};background-image:linear-gradient(90deg,{StoreBrand.PRIMARY} 0%,{StoreBrand.PRIMARY_MID} 55%,{StoreBrand.PRIMARY_BRIGHT} 100%);">&nbsp;</td>
                    </tr>
                    <tr>
                      <td style="padding:26px 28px;color:{StoreBrand.INK};font-size:15px;line-height:1.6;">{TrimLeadingStoreLink(body, storeName)}</td>
                    </tr>
                    <tr>
                      <td style="padding:18px 28px 22px;border-top:1px solid {StoreBrand.LINE};background-color:{StoreBrand.SURFACE_ALT};color:{StoreBrand.INK_MUTED};font-size:12px;line-height:1.7;">{footer}</td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
            """;
    }

    #endregion
}