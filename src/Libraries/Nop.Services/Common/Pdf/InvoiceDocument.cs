using System.ComponentModel;
using System.Linq.Expressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;

namespace Nop.Services.Common.Pdf;

/// <summary>
/// Represents the invoice document
/// </summary>
public partial class InvoiceDocument : PdfDocument<ProductItem>
{
    #region Utilities

    /// <summary>
    /// Build the brand gradient rule - the same rule that sits under the storefront header
    /// </summary>
    /// <param name="height">Rule height, in points</param>
    /// <param name="collSpan">The number of columns occupied by the rule</param>
    /// <returns>A cell for PDF table</returns>
    protected virtual PdfPCell BuildBrandRule(float height = 3.5f, int collSpan = 1)
    {
        return new PdfPCell
        {
            Border = 0,
            Colspan = collSpan,
            FixedHeight = height,
            RunDirection = DocumentRunDirection,
            CellEvent = new BrandRuleCellEvent(Language?.Rtl == true)
        };
    }

    /// <summary>
    /// Build a cell with the given text and font
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="font">Font</param>
    /// <param name="horizontalAlign">Horizontal alignment</param>
    /// <param name="collSpan">The number of columns occupied by a cell</param>
    /// <returns>A cell for PDF table</returns>
    protected virtual PdfPCell BuildStyledCell(string text, Font font, int horizontalAlign = Element.ALIGN_LEFT, int collSpan = 1)
    {
        var cell = new PdfPCell(new Phrase(text, font))
        {
            Border = 0,
            Colspan = collSpan,
            Padding = 3,
            RunDirection = DocumentRunDirection,
            HorizontalAlignment = horizontalAlign,
            VerticalAlignment = Element.ALIGN_CENTER
        };

        cell.SetLeading(0f, PdfDocumentHelper.RELATIVE_LEADING);

        return cell;
    }

    /// <summary>
    /// Set column widths in logical order
    /// </summary>
    /// <param name="grid">PDF table</param>
    /// <param name="widths">Widths, first entry for the column the reader meets first</param>
    protected virtual void SetColumnWidths(PdfGrid grid, float[] widths)
    {
        //an RTL table places its cells right to left, but SetWidths stays positional,
        //so the widths have to be mirrored by hand or every column ends up the wrong size
        grid.SetWidths(Language?.Rtl == true ? widths.Reverse().ToArray() : widths);
    }

    /// <summary>
    /// Get the localized label of the given property
    /// </summary>
    /// <param name="labelSelector">Property selector to get resource key annotation</param>
    /// <returns>Localized label</returns>
    protected virtual string LabelText<TLabel, TOut>(Expression<Func<TLabel, TOut>> labelSelector)
    {
        return LabelField(labelSelector, Font, Language).Content;
    }

    protected virtual PdfGrid CreateAdressesInfo()
    {
        //this store collects a single, shipping-only address - both clones hold it, so print it once
        var addressesTable = PdfDocumentHelper.BuildPdfGrid(numColumns: 1, DocumentRunDirection);
        addressesTable.SpacingAfter = 0;

        BillingAddress.ShippingMethod = ShippingAddress?.ShippingMethod;
        addressesTable.AddCell(PdfDocumentHelper.BuildPdfPCell(BuildAddressTable<InvoiceDocument>(source => BillingAddress, BillingAddress), DocumentRunDirection));

        return addressesTable;
    }

    protected virtual PdfGrid CreateInvoiceHeader()
    {
        var headerTable = PdfDocumentHelper.BuildPdfGrid(numColumns: 2, DocumentRunDirection);
        SetColumnWidths(headerTable, [4, 6]);
        headerTable.SpacingAfter = 18;

        //the logo leads, on the side the reader starts from
        if (LogoData is not null)
        {
            var logo = PdfImageHelper.GetITextSharpImageFromByteArray(LogoData);
            headerTable.AddCell(new PdfPCell(logo, fit: true)
            {
                Border = 0,
                FixedHeight = 70,
                PaddingBottom = 6,
                RunDirection = DocumentRunDirection,
                VerticalAlignment = Element.ALIGN_CENTER,
                HorizontalAlignment = Language.Rtl ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT
            });
        }
        else
        {
            headerTable.AddCell(new PdfPCell(new Phrase())
            {
                Border = 0,
                Padding = 0
            });
        }

        //"Order #1234" doubles as the document title - the resource already carries the wording
        var info = PdfDocumentHelper.BuildPdfGrid(numColumns: 1, DocumentRunDirection);
        info.SpacingAfter = 0;

        var titleFont = PdfDocumentHelper.GetFont(Font, Font.Size * 1.8f, DocumentFontStyle.Bold);
        titleFont.Color = StoreBrand.Primary;

        var titleCell = new PdfPCell(new Phrase { LabelField<InvoiceDocument, string>(source => OrderNumberText, titleFont, Language, OrderNumberText) })
        {
            Border = 0,
            Padding = 3,
            RunDirection = DocumentRunDirection,
            HorizontalAlignment = Element.ALIGN_RIGHT
        };
        titleCell.SetLeading(0f, PdfDocumentHelper.RELATIVE_LEADING);
        info.AddCell(titleCell);

        var metaFont = PdfDocumentHelper.GetFont(Font, Font.Size * 0.95f);
        metaFont.Color = StoreBrand.InkMuted;

        var date = Language.Rtl ? OrderDateUser.FixWeakCharacters() : OrderDateUser;
        info.AddCell(BuildStyledCell($"{LabelText<InvoiceDocument, string>(source => OrderDateUser)}: {date}", metaFont, Element.ALIGN_RIGHT));

        if (!string.IsNullOrEmpty(StoreUrl))
        {
            var linkFont = PdfDocumentHelper.GetFont(Font, Font.Size * 0.95f);
            linkFont.Color = StoreBrand.Primary;

            var linkCell = new PdfPCell(new Phrase { new Anchor(new Chunk(StoreUrl, linkFont)) { Reference = StoreUrl } })
            {
                Border = 0,
                Padding = 3,
                RunDirection = DocumentRunDirection,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            linkCell.SetLeading(0f, PdfDocumentHelper.RELATIVE_LEADING);
            info.AddCell(linkCell);
        }

        headerTable.AddCell(PdfDocumentHelper.BuildPdfPCell(info, DocumentRunDirection, horizontalAlign: Element.ALIGN_RIGHT));

        headerTable.AddCell(BuildBrandRule(collSpan: 2));

        //the address sits in a tinted panel, the surface the storefront uses behind a card
        var addressCell = PdfDocumentHelper.BuildPdfPCell(CreateAdressesInfo(), DocumentRunDirection, collSpan: 2, horizontalAlign: Element.ALIGN_LEFT);
        addressCell.BackgroundColor = StoreBrand.Tint;
        addressCell.Padding = 10;
        headerTable.AddCell(addressCell);

        return headerTable;
    }

    protected virtual PdfGrid CreateFooter(FooterData footerData)
    {
        var footerTable = PdfDocumentHelper.BuildPdfGrid(numColumns: 2, DocumentRunDirection);

        var footerFont = PdfDocumentHelper.GetFont(Font, Font.Size * 0.85f);
        footerFont.Color = StoreBrand.InkMuted;

        footerTable.AddCell(BuildBrandRule(height: 2f, collSpan: 2));

        var footer1Table = PdfDocumentHelper.BuildPdfGrid(numColumns: 1, DocumentRunDirection);
        footer1Table.SpacingAfter = 0;
        foreach (var line in FooterTextColumn1)
            footer1Table.AddCell(BuildStyledCell(line, footerFont));

        var footer2Table = PdfDocumentHelper.BuildPdfGrid(numColumns: 1, DocumentRunDirection);
        footer2Table.SpacingAfter = 0;
        foreach (var line in FooterTextColumn2)
            footer2Table.AddCell(BuildStyledCell(line, footerFont, Element.ALIGN_RIGHT));

        footerTable.AddCell(PdfDocumentHelper.BuildPdfPCell(footer1Table, DocumentRunDirection, horizontalAlign: Element.ALIGN_LEFT));
        footerTable.AddCell(PdfDocumentHelper.BuildPdfPCell(footer2Table, DocumentRunDirection, horizontalAlign: Element.ALIGN_RIGHT));

        footerTable.AddCell(BuildStyledCell($"- {footerData.CurrentPageNumber} -", footerFont, Element.ALIGN_CENTER, collSpan: 2));

        return footerTable;
    }

    /// <summary>
    /// Add a label/value line to the totals box
    /// </summary>
    /// <param name="summaryData">Totals table</param>
    /// <param name="label">Label</param>
    /// <param name="value">Value; empty for a line that already carries its own amount</param>
    /// <param name="emphasized">Whether this is the order total - the one solid brand row</param>
    protected virtual void AddSummaryRow(PdfGrid summaryData, string label, string value, bool emphasized = false)
    {
        var font = PdfDocumentHelper.GetFont(Font, emphasized ? Font.Size * 1.15f : Font.Size, emphasized ? DocumentFontStyle.Bold : DocumentFontStyle.Normal);
        font.Color = emphasized ? StoreBrand.White : StoreBrand.Ink;

        var background = emphasized ? StoreBrand.Primary : StoreBrand.SurfaceAlt;

        foreach (var (text, align) in new[] { (label, Element.ALIGN_LEFT), (value, Element.ALIGN_RIGHT) })
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Border = 0,
                BorderWidthBottom = emphasized ? 0 : 0.7f,
                BorderColorBottom = StoreBrand.Line,
                BackgroundColor = background,
                Padding = 6,
                RunDirection = DocumentRunDirection,
                HorizontalAlignment = align,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            cell.SetLeading(0f, PdfDocumentHelper.RELATIVE_LEADING);
            summaryData.AddCell(cell);
        }
    }

    protected virtual PdfGrid CreateSummary()
    {
        var summaryData = PdfDocumentHelper.BuildPdfGrid(numColumns: 2, DocumentRunDirection);
        SetColumnWidths(summaryData, [6, 4]);

        if (!string.IsNullOrEmpty(Totals.SubTotal))
            AddSummaryRow(summaryData, LabelText<InvoiceTotals, string>(totals => totals.SubTotal), Totals.SubTotal);
        if (!string.IsNullOrEmpty(Totals.Discount))
            AddSummaryRow(summaryData, LabelText<InvoiceTotals, string>(totals => totals.Discount), Totals.Discount);
        if (!string.IsNullOrEmpty(Totals.Shipping))
            AddSummaryRow(summaryData, LabelText<InvoiceTotals, string>(totals => totals.Shipping), Totals.Shipping);
        if (!string.IsNullOrEmpty(Totals.PaymentMethodAdditionalFee))
            AddSummaryRow(summaryData, LabelText<InvoiceTotals, string>(totals => totals.PaymentMethodAdditionalFee), Totals.PaymentMethodAdditionalFee);
        if (!string.IsNullOrEmpty(Totals.Tax))
            AddSummaryRow(summaryData, LabelText<InvoiceTotals, string>(totals => totals.Tax), Totals.Tax);

        //a tax rate line already reads "Tax rate 5%: 1.00", so it fills the label column on its own
        foreach (var rate in Totals.TaxRates)
            AddSummaryRow(summaryData, rate, string.Empty);

        if (!string.IsNullOrEmpty(Totals.OrderTotal))
            AddSummaryRow(summaryData, LabelText<InvoiceTotals, string>(totals => totals.OrderTotal), Totals.OrderTotal, emphasized: true);

        return summaryData;
    }

    protected virtual PdfGrid CreateCheckoutAttributes()
    {
        var attributesData = PdfDocumentHelper.BuildPdfGrid(numColumns: 1, DocumentRunDirection);

        var font = PdfDocumentHelper.GetFont(Font, Font.Size * 0.95f);
        font.Color = StoreBrand.InkMuted;

        attributesData.AddCell(BuildStyledCell(CheckoutAttributes, font));

        return attributesData;
    }

    protected virtual PdfGrid CreateOrderNotes()
    {
        var notesTable = PdfDocumentHelper.BuildPdfGrid(numColumns: 2, DocumentRunDirection);

        if (OrderNotes?.Any() != true)
            return notesTable;

        SetColumnWidths(notesTable, [2, 5]);

        var fontBold = PdfDocumentHelper.GetFont(Font, Font.Size * 1.1f, DocumentFontStyle.Bold);
        fontBold.Color = StoreBrand.Primary;
        var label = LabelField<InvoiceDocument, List<(string, string)>>(invoice => invoice.OrderNotes, fontBold, Language);

        notesTable.AddCell(
            new PdfPCell(new Phrase(label))
            {
                Border = 0,
                BorderWidthBottom = 1.5f,
                BorderColorBottom = StoreBrand.Primary,
                Colspan = 2,
                HorizontalAlignment = Element.ALIGN_LEFT,
                PaddingBottom = 5,
                RunDirection = DocumentRunDirection,
            });

        var noteFont = PdfDocumentHelper.GetFont(Font, Font.Size * 0.95f);
        var dateFont = PdfDocumentHelper.GetFont(Font, Font.Size * 0.95f);
        dateFont.Color = StoreBrand.InkMuted;

        foreach (var (date, note) in OrderNotes)
        {
            notesTable.AddCell(BuildStyledCell(Language.Rtl ? date.FixWeakCharacters() : date, dateFont));
            notesTable.AddCell(BuildStyledCell(note, noteFont));
        }

        return notesTable;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Generate the invoice
    /// </summary>
    /// <param name="pdfStreamOutput">Stream for PDF output</param>
    public override void Generate(Stream pdfStreamOutput)
    {
        Document
            .MainTablePreferences(table =>
            {
                table.ColumnsWidthsType(TableColumnWidthType.Relative);
            })
            .MainTableDataSource(dataSource =>
            {
                dataSource.StronglyTypedList(Products);
            })
            .PagesFooter(footer =>
            {
                footer.InlineFooter(inlineFooter =>
                {
                    inlineFooter.FooterProperties(new FooterBasicProperties
                    {
                        PdfFont = footer.PdfFont,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        RunDirection = Language.Rtl ? PdfRunDirection.RightToLeft : PdfRunDirection.LeftToRight
                    });
                    inlineFooter.AddPageFooter(data => CreateFooter(data));
                });
            })
            .MainTableColumns(columns =>
            {
                columns.AddColumn(column => ConfigureProductColumn(column, p => p.Name, width: 9, printProductAttributes: true));
                if (ShowSkuInProductList)
                    columns.AddColumn(column => ConfigureProductColumn(column, p => p.Sku, width: 3));
                if (ShowVendorInProductList)
                    columns.AddColumn(column => ConfigureProductColumn(column, p => p.VendorName, width: 3));
                columns.AddColumn(column => ConfigureProductColumn(column, p => p.Price, width: 4));
                columns.AddColumn(column => ConfigureProductColumn(column, p => p.Quantity, width: 2));
                columns.AddColumn(column => ConfigureProductColumn(column, p => p.Total, width: 4));
            })
            .MainTableEvents(events =>
            {
                events.MainTableCreated(events =>
                {
                    //add to body, since adding hyperlinks to document header is not allowed
                    events.PdfDoc.Add(CreateInvoiceHeader());
                });
                events.MainTableAdded(events =>
                {
                    var summaryTable = PdfDocumentHelper.BuildPdfGrid(numColumns: 3, DocumentRunDirection);
                    SetColumnWidths(summaryTable, [3, 2, 5]);
                    summaryTable.AddCell(PdfDocumentHelper.BuildPdfPCell(CreateCheckoutAttributes(), DocumentRunDirection, 3, horizontalAlign: Element.ALIGN_LEFT));

                    summaryTable.AddCell(new PdfPCell() { Colspan = 2, Border = 0 });
                    summaryTable.AddCell(PdfDocumentHelper.BuildPdfPCell(CreateSummary(), DocumentRunDirection));

                    events.PdfDoc.Add(summaryTable);
                    events.PdfDoc.Add(CreateOrderNotes());
                });
            })
            .Generate(builder => builder.AsPdfStream(pdfStreamOutput, closeStream: false));
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the logo binary
    /// </summary>
    public byte[] LogoData { get; set; }

    /// <summary>
    /// Gets or sets the date and time of order creation
    /// </summary>
    [DisplayName("Pdf.OrderDate")]
    public required string OrderDateUser { get; init; }

    /// <summary>
    /// Gets or sets the order number
    /// </summary>
    [DisplayName("Pdf.Order")]
    public required string OrderNumberText { get; init; }

    /// <summary>
    /// Gets or sets store location
    /// </summary>
    public string StoreUrl { get; init; }

    /// <summary>
    /// Gets or sets the order address
    /// </summary>
    [DisplayName("Pdf.ShippingInformation")]
    public required AddressItem BillingAddress { get; init; }

    /// <summary>
    /// Gets or sets the shipping address
    /// </summary>
    [DisplayName("Pdf.ShippingInformation")]
    public AddressItem ShippingAddress { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to display product SKU in the invoice document
    /// </summary>
    public bool ShowSkuInProductList { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to display vendor name in the invoice document
    /// </summary>
    public bool ShowVendorInProductList { get; set; }

    /// <summary>
    /// Gets or sets the checkout attribute description
    /// </summary>
    public string CheckoutAttributes { get; set; }

    /// <summary>
    /// Gets or sets order totals
    /// </summary>
    public InvoiceTotals Totals { get; set; } = new();

    /// <summary>
    /// Gets or sets order notes
    /// </summary>
    [DisplayName("Pdf.OrderNotes")]
    public List<(string, string)> OrderNotes { get; set; }

    /// <summary>
    /// Gets or sets the text that will appear at the bottom of invoice (column 1)
    /// </summary>
    public List<string> FooterTextColumn1 { get; set; } = new();

    /// <summary>
    /// Gets or sets the text that will appear at the bottom of invoice (column 2)
    /// </summary>
    public List<string> FooterTextColumn2 { get; set; } = new();

    #endregion
}