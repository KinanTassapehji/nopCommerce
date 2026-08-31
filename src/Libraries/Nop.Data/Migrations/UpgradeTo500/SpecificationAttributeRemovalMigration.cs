using FluentMigrator;
using LinqToDB;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Migrations.UpgradeTo500;

/// <summary>
/// Moves every product specification onto a product attribute and drops the
/// specification tables.
///
/// TmTm never sold on specifications - the four attributes in use (colour,
/// material, care, warranty) are facts about the product, not a second, parallel
/// way of describing one. Rather than lose them with the feature, each one lands
/// as a read-only checkbox attribute: pre-selected, disabled in the storefront,
/// so it still reads as "this product is stoneware" and not as a choice the
/// shopper has to make.
///
/// A product that already carries a purchasable attribute of the same name (the
/// colour picker on the bedding, say) keeps that one; the specification row for
/// it would only duplicate the same word twice on the page.
///
/// The specification entity classes are gone by the time this runs, so the three
/// tables are read through the local <see cref="LegacyTables"/> types below.
/// LinqToDB maps a BaseEntity to a table of the same name, which is why they are
/// named after the tables rather than after the entities they replace.
/// </summary>
[NopSchemaMigration("2026-08-31 00:00:01", "SchemaMigration for 5.00.0 - convert product specifications to product attributes")]
public class SpecificationAttributeRemovalMigration : ForwardOnlyMigration
{
    #region Constants

    /// <summary>
    /// AttributeControlType.ReadonlyCheckboxes - the one control type that shows a
    /// value without asking the shopper to pick it
    /// </summary>
    protected const int READONLY_CHECKBOXES = 50;

    #endregion

    #region Fields

    protected readonly INopDataProvider _dataProvider;

    #endregion

    #region Ctor

    public SpecificationAttributeRemovalMigration(INopDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        //a store installed after the feature was removed has nothing to convert
        if (Schema.Table("Product_SpecificationAttribute_Mapping").Exists())
            ConvertToProductAttributes();

        foreach (var table in new[]
                 {
                     "Product_SpecificationAttribute_Mapping",
                     "SpecificationAttributeOption",
                     "SpecificationAttribute",
                     "SpecificationAttributeGroup"
                 })
            if (Schema.Table(table).Exists())
                Delete.Table(table);
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Rewrites every specification mapping as a read-only product attribute
    /// </summary>
    protected virtual void ConvertToProductAttributes()
    {
        var options = _dataProvider.GetTable<LegacyTables.SpecificationAttributeOption>().ToList();
        var specifications = _dataProvider.GetTable<LegacyTables.SpecificationAttribute>()
            .ToDictionary(sa => sa.Id, sa => sa.Name);

        var mappings = _dataProvider.GetTable<LegacyTables.Product_SpecificationAttribute_Mapping>()
            .OrderBy(psa => psa.ProductId).ThenBy(psa => psa.DisplayOrder)
            .ToList();

        if (mappings.Count == 0)
            return;

        //reuse the product attribute of the same name where the store already has one
        var attributeIdByName = _dataProvider.GetTable<ProductAttribute>()
            .ToList()
            .GroupBy(pa => pa.Name, StringComparer.InvariantCultureIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.InvariantCultureIgnoreCase);

        //the mappings a product already has, so an existing purchasable attribute wins
        var existingMappings = _dataProvider.GetTable<ProductAttributeMapping>()
            .Select(pam => new { pam.ProductId, pam.ProductAttributeId })
            .ToList()
            .Select(pam => (pam.ProductId, pam.ProductAttributeId))
            .ToHashSet();

        var optionById = options.ToDictionary(o => o.Id);

        foreach (var psa in mappings)
        {
            if (!optionById.TryGetValue(psa.SpecificationAttributeOptionId, out var option) ||
                !specifications.TryGetValue(option.SpecificationAttributeId, out var name) ||
                string.IsNullOrWhiteSpace(name))
                continue;

            //the value is the option name, except for the free-text/hyperlink types
            //which kept theirs in CustomValue
            var value = psa.AttributeTypeId == 0 ? option.Name : psa.CustomValue;
            if (string.IsNullOrWhiteSpace(value))
                continue;

            if (!attributeIdByName.TryGetValue(name, out var productAttributeId))
            {
                productAttributeId = _dataProvider.InsertEntity(new ProductAttribute { Name = name }).Id;
                attributeIdByName[name] = productAttributeId;
            }

            if (!existingMappings.Add((psa.ProductId, productAttributeId)))
                continue;

            var mapping = _dataProvider.InsertEntity(new ProductAttributeMapping
            {
                ProductId = psa.ProductId,
                ProductAttributeId = productAttributeId,
                AttributeControlTypeId = READONLY_CHECKBOXES,
                IsRequired = false,
                DisplayOrder = psa.DisplayOrder
            });

            _dataProvider.InsertEntity(new ProductAttributeValue
            {
                ProductAttributeMappingId = mapping.Id,
                Name = value,
                ColorSquaresRgb = option.ColorSquaresRgb,
                IsPreSelected = true,
                DisplayOrder = psa.DisplayOrder
            });
        }
    }

    #endregion

    #region Nested classes

    /// <summary>
    /// Minimal stand-ins for the deleted specification entities, so this migration
    /// can still read the three tables it is about to drop
    /// </summary>
    protected static class LegacyTables
    {
        public partial class SpecificationAttribute : BaseEntity
        {
            public string Name { get; set; }
        }

        public partial class SpecificationAttributeOption : BaseEntity
        {
            public int SpecificationAttributeId { get; set; }
            public string Name { get; set; }
            public string ColorSquaresRgb { get; set; }
        }

        public partial class Product_SpecificationAttribute_Mapping : BaseEntity
        {
            public int ProductId { get; set; }
            public int SpecificationAttributeOptionId { get; set; }
            public int AttributeTypeId { get; set; }
            public string CustomValue { get; set; }
            public int DisplayOrder { get; set; }
        }
    }

    #endregion
}