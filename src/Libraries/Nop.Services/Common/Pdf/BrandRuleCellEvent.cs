using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Nop.Services.Common.Pdf;

/// <summary>
/// Paints the brand gradient behind a cell, so a document can carry the same
/// gradient rule the storefront header does.
///
/// iTextSharp shades between two stops only, and the brand gradient has three
/// (primary at 0%, mid at 55%, bright at 100%), so it is painted as two
/// abutting shadings.
/// </summary>
public partial class BrandRuleCellEvent : IPdfPCellEvent
{
    #region Fields

    protected readonly bool _rtl;

    #endregion

    #region Ctor

    /// <param name="rtl">Run direction of the document - the gradient starts on the side the reader starts</param>
    public BrandRuleCellEvent(bool rtl = false)
    {
        _rtl = rtl;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Fill a horizontal band with an axial shading
    /// </summary>
    protected virtual void Paint(PdfContentByte canvas, Rectangle position, float from, float to, BaseColor startColor, BaseColor endColor)
    {
        var shading = PdfShading.SimpleAxial(canvas.PdfWriter, from, position.Bottom, to, position.Bottom, startColor, endColor);
        var pattern = new PdfShadingPattern(shading);

        canvas.SaveState();
        canvas.Rectangle(from, position.Bottom, to - from, position.Height);
        canvas.Clip();
        canvas.NewPath();
        canvas.SetShadingFill(pattern);
        canvas.Rectangle(from, position.Bottom, to - from, position.Height);
        canvas.Fill();
        canvas.RestoreState();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Draw the rule
    /// </summary>
    public virtual void CellLayout(PdfPCell cell, Rectangle position, PdfContentByte[] canvases)
    {
        var canvas = canvases[PdfPTable.BACKGROUNDCANVAS];

        var start = _rtl ? StoreBrand.PrimaryBright : StoreBrand.Primary;
        var end = _rtl ? StoreBrand.Primary : StoreBrand.PrimaryBright;
        var breakpoint = position.Left + position.Width * (_rtl ? 0.45f : 0.55f);

        Paint(canvas, position, position.Left, breakpoint, start, StoreBrand.PrimaryMid);
        Paint(canvas, position, breakpoint, position.Right, StoreBrand.PrimaryMid, end);
    }

    #endregion
}