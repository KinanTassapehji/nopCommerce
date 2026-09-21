using System.Text;
using FluentAssertions;
using iTextSharp.text;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Orders;
using NUnit.Framework;

namespace Nop.Tests.Nop.Services.Tests.Common;

[TestFixture]
public class PdfServiceTests : ServiceTest
{
    private IOrderService _orderService;
    private IPdfService _pdfService;

    [OneTimeSetUp]
    public void SetUp()
    {
        //fonts are registered by UseNopPdf at application start, which a test never runs
        var fileProvider = GetService<INopFileProvider>();
        foreach (var path in fileProvider.EnumerateFiles(fileProvider.MapPath("~/App_Data/Pdf/"), "*.ttf"))
            FontFactory.Register(path, fileProvider.GetFileNameWithoutExtension(path));

        _orderService = GetService<IOrderService>();
        _pdfService = GetService<IPdfService>();
    }

    /// <summary>
    /// The branded invoice paints a gradient rule through a cell event, a tinted
    /// address panel and a totals box - none of which iTextSharp objects to until
    /// it renders. This is the check that it renders.
    /// </summary>
    [Test]
    public async Task CanPrintOrderToPdf()
    {
        var order = await _orderService.GetOrderByIdAsync(1);

        await using var stream = new MemoryStream();
        await _pdfService.PrintOrderToPdfAsync(stream, order);

        var invoice = stream.ToArray();

        Encoding.ASCII.GetString(invoice, 0, 5).Should().Be("%PDF-");
        invoice.Length.Should().BeGreaterThan(1000);
    }
}