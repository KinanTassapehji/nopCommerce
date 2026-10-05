using iTextSharp.text;

namespace Nop.Services.Common;

/// <summary>
/// The Loli Store brand palette, mirrored from Themes/Loli/Content/css/tokens.css.
/// The invoice PDF and the outgoing e-mails read their colours from here, so a
/// printed order, a notification and the storefront all carry one identity.
///
/// This is the only per-store file in that pipeline - Arabia holds its own copy
/// with its own values; every other file in it is identical in both forks.
/// </summary>
public static partial class StoreBrand
{
    #region Constants

    /// <summary>
    /// Primary brand colour. Holds white text at 5.43:1, so it is the only one
    /// allowed under a label
    /// </summary>
    public const string PRIMARY = "#cf0c5e";

    /// <summary>
    /// Middle stop of the brand gradient
    /// </summary>
    public const string PRIMARY_MID = "#eb3274";

    /// <summary>
    /// End stop of the brand gradient. Decorative only - 2.13:1 on white, it
    /// never carries text
    /// </summary>
    public const string PRIMARY_BRIGHT = "#ff8fb5";

    /// <summary>
    /// Body text
    /// </summary>
    public const string INK = "#24121a";

    /// <summary>
    /// Secondary text - field labels, footnotes, the mail footer
    /// </summary>
    public const string INK_MUTED = "#6b5a61";

    /// <summary>
    /// Hairlines: table rules, panel edges
    /// </summary>
    public const string LINE = "#efe1e7";

    /// <summary>
    /// Panel fill behind the address block and the totals box
    /// </summary>
    public const string TINT = "#fdebf2";

    /// <summary>
    /// Quieter alternate surface - zebra rows, the mail page background
    /// </summary>
    public const string SURFACE_ALT = "#fcf6f8";

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