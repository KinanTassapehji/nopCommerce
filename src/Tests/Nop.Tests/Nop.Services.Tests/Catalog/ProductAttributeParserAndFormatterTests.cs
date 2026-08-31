using FluentAssertions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using NUnit.Framework;

namespace Nop.Tests.Nop.Services.Tests.Catalog;

[TestFixture]
public class ProductAttributeParserTests : ServiceTest
{
    private IProductAttributeParser _productAttributeParser;
    private IProductAttributeFormatter _productAttributeFormatter;
    private IEnumerable<KeyValuePair<ProductAttributeMapping, IList<ProductAttributeValue>>> _productAttributeMappings;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        var productAttributeService = GetService<IProductAttributeService>();

        _productAttributeParser = GetService<IProductAttributeParser>();
        _productAttributeFormatter = GetService<IProductAttributeFormatter>();

        var product = await GetService<IProductService>()
            .GetProductBySkuAsync("TM-AW-701");
        var mappings = await productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
        _productAttributeMappings = await mappings.SelectAwait(async p => KeyValuePair.Create(p, await productAttributeService.GetProductAttributeValuesAsync(p.Id))).ToListAsync();
    }

    [Test]
    public async Task CanAddAndParseProductAttributes()
    {
        var attributes = string.Empty;

        foreach (var productAttributeMapping in _productAttributeMappings)
        {
            var skip = true;

            foreach (var productAttributeValue in productAttributeMapping.Value.OrderBy(p => p.Id))
            {
                if (skip)
                {
                    skip = false;
                    continue;
                }

                attributes = _productAttributeParser.AddProductAttribute(attributes, productAttributeMapping.Key, productAttributeValue.Id.ToString());
            }
        }

        var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributes);

        var parsedAttributeValues = attributeValues.Select(p => p.Id).ToList();

        foreach (var productAttributeMapping in _productAttributeMappings)
        {
            var skip = true;

            foreach (var productAttributeValue in productAttributeMapping.Value.OrderBy(p => p.Id))
            {
                if (skip)
                {
                    parsedAttributeValues.Contains(productAttributeValue.Id).Should().BeFalse();
                    skip = false;
                    continue;
                }

                parsedAttributeValues.Contains(productAttributeValue.Id).Should().BeTrue();
            }
        }
    }

    [Test]
    public async Task CanAddAndRemoveProductAttributes()
    {
        var attributes = string.Empty;

        var delete = false;

        foreach (var productAttributeMapping in _productAttributeMappings)
        {
            foreach (var productAttributeValue in productAttributeMapping.Value.OrderBy(p => p.Id))
                attributes = _productAttributeParser.AddProductAttribute(attributes, productAttributeMapping.Key, productAttributeValue.Id.ToString());

            if (delete)
                attributes = _productAttributeParser.RemoveProductAttribute(attributes, productAttributeMapping.Key);

            delete = !delete;
        }

        var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributes);

        var parsedAttributeValues = attributeValues.Select(p => p.Id).ToList();

        delete = false;

        foreach (var productAttributeMapping in _productAttributeMappings)
        {
            foreach (var productAttributeValue in productAttributeMapping.Value.OrderBy(p => p.Id))
                parsedAttributeValues.Contains(productAttributeValue.Id).Should().Be(!delete);

            delete = !delete;
        }
    }

    [Test]
    public async Task CanRenderAttributesWithoutPrices()
    {
        var attributes = string.Empty;

        foreach (var productAttributeMapping in _productAttributeMappings)
        foreach (var productAttributeValue in productAttributeMapping.Value.OrderBy(p => p.Id))
            attributes = _productAttributeParser.AddProductAttribute(attributes, productAttributeMapping.Key, productAttributeValue.Id.ToString());

        var product = new Product();
        var customer = new Customer();
        var store = new Store();

        var formattedAttributes = await _productAttributeFormatter.FormatAttributesAsync(product,
            attributes, customer, store, "<br />", false);

        formattedAttributes.Should().Be(
            "المقاس: صغير جداً<br />المقاس: صغير<br />المقاس: وسط<br />المقاس: كبير<br />المقاس: كبير جداً<br />اللون: تركوازي غامق<br />اللون: رملي<br />اللون: فحمي");

        formattedAttributes = await _productAttributeFormatter.FormatAttributesAsync(product,
            attributes, customer, store, "<br />", false, false);

        formattedAttributes.Should().Be(
            "المقاس: صغير جداً<br />المقاس: صغير<br />المقاس: وسط<br />المقاس: كبير<br />المقاس: كبير جداً<br />اللون: تركوازي غامق<br />اللون: رملي<br />اللون: فحمي");
    }

}