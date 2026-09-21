using iTextSharp.text;

namespace Nop.Services.Common;

/// <summary>
/// The Arabia brand palette, mirrored from Themes/Arabia/Content/css/tokens.css.
/// The invoice PDF and the outgoing e-mails read their colours from here, so a
/// printed order, a notification and the storefront all carry one identity.
///
/// This is the only per-store file in that pipeline - TmTm holds its own copy
/// with its own values; every other file in it is identical in both forks.
/// </summary>
public static partial class StoreBrand
{
    #region Constants

    /// <summary>
    /// Primary brand colour. Holds white text at 4.78:1, so it is the only one
    /// allowed under a label
    /// </summary>
    public const string PRIMARY = "#0D77BD";

    /// <summary>
    /// Middle stop of the brand gradient
    /// </summary>
    public const string PRIMARY_MID = "#1E8FD5";

    /// <summary>
    /// End stop of the brand gradient. Decorative only - 2.26:1 on white, it
    /// never carries text
    /// </summary>
    public const string PRIMARY_BRIGHT = "#5BB6E8";

    /// <summary>
    /// Body text
    /// </summary>
    public const string INK = "#0C2536";

    /// <summary>
    /// Secondary text - field labels, footnotes, the mail footer
    /// </summary>
    public const string INK_MUTED = "#5A6B7A";

    /// <summary>
    /// Hairlines: table rules, panel edges
    /// </summary>
    public const string LINE = "#DDE4EA";

    /// <summary>
    /// Panel fill behind the address block and the totals box
    /// </summary>
    public const string TINT = "#E0F2FF";

    /// <summary>
    /// Quieter alternate surface - zebra rows, the mail page background
    /// </summary>
    public const string SURFACE_ALT = "#F2F7FB";

    /// <summary>
    /// Paper
    /// </summary>
    public const string WHITE = "#ffffff";

    #endregion

    #region Utilities

    /// <summary>
    /// Convert a "#rrggbb" literal to the iTextSharp colour the PDF layer wants
    /// </summary>
    /// <param name="hex">Colour as declared above</param>
    /// <returns>A colour</returns>
    private static BaseColor Rgb(string hex)
    {
        return new BaseColor(
            Convert.ToInt32(hex.Substring(1, 2), 16),
            Convert.ToInt32(hex.Substring(3, 2), 16),
            Convert.ToInt32(hex.Substring(5, 2), 16));
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the primary brand colour
    /// </summary>
    public static BaseColor Primary { get; } = Rgb(PRIMARY);

    /// <summary>
    /// Gets the middle stop of the brand gradient
    /// </summary>
    public static BaseColor PrimaryMid { get; } = Rgb(PRIMARY_MID);

    /// <summary>
    /// Gets the end stop of the brand gradient
    /// </summary>
    public static BaseColor PrimaryBright { get; } = Rgb(PRIMARY_BRIGHT);

    /// <summary>
    /// Gets the body text colour
    /// </summary>
    public static BaseColor Ink { get; } = Rgb(INK);

    /// <summary>
    /// Gets the secondary text colour
    /// </summary>
    public static BaseColor InkMuted { get; } = Rgb(INK_MUTED);

    /// <summary>
    /// Gets the hairline colour
    /// </summary>
    public static BaseColor Line { get; } = Rgb(LINE);

    /// <summary>
    /// Gets the panel fill
    /// </summary>
    public static BaseColor Tint { get; } = Rgb(TINT);

    /// <summary>
    /// Gets the alternate surface
    /// </summary>
    public static BaseColor SurfaceAlt { get; } = Rgb(SURFACE_ALT);

    /// <summary>
    /// Gets paper white
    /// </summary>
    public static BaseColor White { get; } = Rgb(WHITE);

    #endregion
}