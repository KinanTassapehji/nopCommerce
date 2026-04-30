using System.Globalization;
using System.Text.Encodings.Web;
using System.Xml;
using System.Xml.Linq;
using Nop.Core;
using Nop.Plugin.Misc.PunchOut.Domain.CXML;

namespace Nop.Plugin.Misc.PunchOut.Services;

/// <summary>
/// Provides functionality for building XML documents for PunchOut integration scenarios
/// </summary>
public class PunchOutXmlBuilder
{
    private static XmlReaderSettings CreateSecureSettings()
    {
        return new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            MaxCharactersFromEntities = 0
        };
    }

    /// <summary>
    /// Parses a PunchOut setup request
    /// </summary>
    /// <param name="xml">The XML string to parse</param>
    /// <returns>A PunchOutSetupRequest object containing the parsed values</returns>
    public PunchOutSetupRequest ParseSetupRequest(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, CreateSecureSettings());
        var document = XDocument.Load(xmlReader);
        var root = document.Element("cXML") ?? throw new NopException("cXML root element missing.");

        var request = root.Element("Request")?.Element("PunchOutSetupRequest")
            ?? throw new NopException("PunchOutSetupRequest missing.");

        var credential = request.Parent?.Parent?
            .Element("Header")?.Element("Sender")?.Element("Credential");

        var contactEmail = request.Element("Contact")?.Element("Email")?.Value
            ?? request.Elements("Extrinsic").Where(x => x.Attribute("name")?.Value == "UserEmail").FirstOrDefault()?.Value;

        return new PunchOutSetupRequest
        {
            PayloadId = (string)root.Attribute("payloadID"),
            TimestampUtc = DateTime.Parse((string)root.Attribute("timestamp"), null, DateTimeStyles.AdjustToUniversal),
            Identity = credential?.Element("Identity")?.Value,
            SharedSecret = credential?.Element("SharedSecret")?.Value,
            BuyerCookie = request.Element("BuyerCookie")?.Value,
            BrowserFormPostUrl = request.Element("BrowserFormPost")?.Element("URL")?.Value,
            Contact = contactEmail
        };
    }

    /// <summary>
    /// Parses a PunchOut order request XML and extracts relevant information into a PunchOutOrderRequest object
    /// </summary>
    /// <param name="xml">Raw XML string</param>
    public PunchOutOrderRequest ParseOrderRequest(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, CreateSecureSettings());
        var document = XDocument.Load(xmlReader);
        var root = document.Element("cXML") ?? throw new NopException("cXML root element missing.");

        var request = root.Element("Request")?.Element("OrderRequest")
            ?? throw new NopException("OrderRequest missing.");

        var credential = root.Element("Header")?.Element("Sender")?.Element("Credential");

        var orderRequest = new PunchOutOrderRequest
        {
            PayloadId = (string)root.Attribute("payloadID") ?? Guid.NewGuid().ToString(),
            TimestampUtc = DateTime.TryParse((string)root.Attribute("timestamp"), null, DateTimeStyles.AdjustToUniversal, out var timestamp)
                ? timestamp
                : DateTime.UtcNow,
            Identity = credential?.Element("Identity")?.Value,
            SharedSecret = credential?.Element("SharedSecret")?.Value
        };

        // parse OrderRequestHeader
        var orderHeader = request.Element("OrderRequestHeader");
        if (orderHeader != null)
        {
            orderRequest.CurrencyCode = orderHeader.Element("Total")?.Element("Money")?.Attribute("currency")?.Value ?? "USD";
            orderRequest.OrderID = orderHeader.Attribute("orderID")?.Value;

            // parse total
            var total = orderHeader.Element("Total")?.Element("Money")?.Value;
            if (decimal.TryParse(total, CultureInfo.InvariantCulture, out var totalAmount))
                orderRequest.Total = totalAmount;

            // parse Contact
            orderRequest.Contact = orderHeader.Element("Contact")?.Element("Email")?.Value
                ?? request.Elements("Extrinsic").Where(x => x.Attribute("name")?.Value == "UserEmail").FirstOrDefault()?.Value;

            // parse BillTo
            var billTo = orderHeader.Element("BillTo");
            if (billTo != null)
            {
                orderRequest.BillTo = ParseAddress(billTo);
            }

            // parse ShipTo
            var shipTo = orderHeader.Element("ShipTo");
            if (shipTo != null)
            {
                orderRequest.ShipTo = ParseAddress(shipTo);
            }
        }

        // parse ItemOut elements
        var itemsOut = request.Descendants("ItemOut");
        foreach (var itemOut in itemsOut)
        {
            var lineItem = ParseLineItem(itemOut);
            if (lineItem != null)
                orderRequest.LineItems.Add(lineItem);
        }

        return orderRequest;
    }

    /// <summary>
    /// Parses address information from XML element
    /// </summary>
    /// <param name="addressElement">The XML element containing address information</param>
    /// <returns>The parsed address object</returns>
    private PunchOutAddress ParseAddress(XElement addressElement)
    {
        var address = new PunchOutAddress();

        var addressInfo = addressElement.Element("Address");
        if (addressInfo != null)
        {
            var name = addressInfo.Element("Name")?.Value;
            var email = addressInfo.Element("Email")?.Value;
            var phone = addressInfo.Element("Phone")?.Value;
            var company = addressInfo.Attribute("addressID")?.Value;

            address.Name = name;
            address.Email = email;
            address.PhoneNumber = phone;
            address.Company = company;
            address.Address1 = addressInfo.Element("Street")?.Value;
            address.City = addressInfo.Element("City")?.Value;
            address.State = addressInfo.Element("State")?.Value;
            address.PostalCode = addressInfo.Element("PostalCode")?.Value;
            address.Country = addressInfo.Element("Country")?.Attribute("isoCountryCode")?.Value;
        }

        return address;
    }

    /// <summary>
    /// Parses a line item from ItemIn element
    /// </summary>
    /// <param name="itemIn">The XML element containing the line item information</param>
    /// <returns>The parsed line item object</returns>
    private PunchOutOrderLineItem ParseLineItem(XElement itemIn)
    {
        var quantity = itemIn.Attribute("quantity")?.Value;
        if (!int.TryParse(quantity, out var qty))
            return null;

        var itemId = itemIn.Element("ItemID");
        var supplierPartId = itemId?.Element("SupplierPartID")?.Value;

        var itemDetail = itemIn.Element("ItemDetail");
        var unitPrice = itemDetail?.Element("UnitPrice")?.Element("Money")?.Value;
        var currency = itemDetail?.Element("UnitPrice")?.Element("Money")?.Attribute("currency")?.Value ?? "USD";
        var description = itemDetail?.Element("Description")?.Value;
        var unitOfMeasure = itemDetail?.Element("UnitOfMeasure")?.Value ?? "EA";

        if (!decimal.TryParse(unitPrice, CultureInfo.InvariantCulture, out var price))
            price = 0m;

        return new PunchOutOrderLineItem
        {
            SupplierPartId = supplierPartId,
            Description = description,
            Quantity = qty,
            UnitPrice = price,
            CurrencyCode = currency,
            UnitOfMeasure = unitOfMeasure
        };
    }

    /// <summary>
    /// Builds a PunchOut setup response XML string based on the provided model
    /// </summary>
    /// <param name="model">The setup response model</param>
    /// <returns>The XML string</returns>
    public string BuildSetupResponse(PunchOutSetupResponse model)
    {
        var document =
        new XDocument(
            new XElement("cXML",
                new XAttribute("payloadID", Guid.NewGuid().ToString("N")),
                new XAttribute("timestamp", DateTime.UtcNow.ToString("o")),

                new XElement("Response",
                    new XElement("Status",
                        new XAttribute("code", "200"),
                        new XAttribute("text", "OK")),
                    new XElement("PunchOutSetupResponse",
                        new XElement("StartPage",
                            new XElement("URL", model.StartPageUrl)
                        )
                    )
                )
            )
        );
        return document.ToString(SaveOptions.DisableFormatting);
    }

    /// <summary>
    /// Builds a PunchOut order response XML string
    /// </summary>
    /// <param name="model">The order response model</param>
    /// <returns>The XML string</returns>
    public string BuildOrderResponse(PunchOutOrderResponse model)
    {
        var document =
        new XDocument(
            new XElement("cXML",
                new XAttribute("payloadID", Guid.NewGuid().ToString("N")),
                new XAttribute("timestamp", DateTime.UtcNow.ToString("o")),

                new XElement("Response",
                    new XElement("Status",
                        new XAttribute("code", model.StatusCode),
                        new XAttribute("text", model.StatusText)
                    )
                )
            )
        );
        return document.ToString(SaveOptions.DisableFormatting);
    }

    /// <summary>
    /// Builds a PunchOut error response XML string
    /// </summary>
    /// <param name="model">The error response model</param>
    /// <returns>The XML string</returns>
    public string BuildErrorResponse(PunchOutErrorResponse model)
    {
        var document =
        new XDocument(
            new XElement("cXML",
                new XAttribute("payloadID", Guid.NewGuid().ToString("N")),
                new XAttribute("timestamp", DateTime.UtcNow.ToString("o")),

                new XElement("Response",
                    new XElement("Status",
                        new XAttribute("code", model.StatusCode),
                        new XAttribute("text", model.StatusText)
                    ),
                    model.ErrorMessage
                )
            )
        );

        return document.ToString(SaveOptions.DisableFormatting);
    }

    /// <summary>
    /// Builds a PunchOut order message XML string
    /// </summary>
    /// <param name="model">The order message model</param>
    /// <returns>The XML string</returns>
    public string BuildPunchOutOrderMessage(PunchOutOrderMessage model)
    {
        var document =
            new XDocument(
                new XElement("cXML",
                    new XAttribute("payloadID", Guid.NewGuid().ToString("N")),
                    new XAttribute("timestamp", DateTime.UtcNow.ToString("o")),

                    new XElement("Message",
                        new XElement("PunchOutOrderMessage",
                            new XElement("BuyerCookie", model.BuyerCookie),
                            new XElement("PunchOutOrderMessageHeader",
                                new XAttribute("operationAllowed", "create"),
                                new XElement("Total",
                                    new XElement("Money",
                                        new XAttribute("currency", model.Items.FirstOrDefault()?.CurrencyCode ?? "USD"),
                                        model.Total.ToString("F2", CultureInfo.InvariantCulture)
                                    )
                                )
                            ),

                            model.Items.Select(item =>
                            new XElement("ItemIn",
                                new XAttribute("quantity", item.Quantity),
                                new XElement("ItemID",
                                    new XElement("SupplierPartID", item.SupplierPartId)),
                                new XElement("ItemDetail",
                                    new XElement("UnitPrice",
                                        new XElement("Money",
                                            new XAttribute("currency", item.CurrencyCode),
                                            item.UnitPrice)),

                                    new XElement("Description", item.Description),
                                    new XElement("UnitOfMeasure", item.UnitOfMeasure))
                                )
                            )
                        )
                    )
                )
            );

        return document.ToString(SaveOptions.DisableFormatting);
    }

    /// <summary>
    /// Builds an HTML form that auto-submits the provided cXML to the specified return URL
    /// </summary>
    /// <param name="returnUrl">Url</param>
    /// <param name="cxml">cXML</param>
    /// <returns>HTML form</returns>
    public string BuildAutoSubmitForm(string returnUrl, string cxml)
    {
        var encodedUrl = HtmlEncoder.Default.Encode(returnUrl);
        var encodedXml = HtmlEncoder.Default.Encode(cxml);

        return $"""
            <!DOCTYPE html>
            <html>
                <head>
                    <meta charset="utf-8"/>
                    <title>PunchOut Return</title>
                </head>

                <body onload="document.forms[0].submit();">
                    <form method="POST" action="{encodedUrl}">
                    <input type="hidden" name="cXML-urlencoded" value="{encodedXml}" />

                    <noscript>
                        <button type="submit">
                            Continue
                        </button>
                    </noscript>
                    </form>
                </body>
            </html>
            """;
    }
}
