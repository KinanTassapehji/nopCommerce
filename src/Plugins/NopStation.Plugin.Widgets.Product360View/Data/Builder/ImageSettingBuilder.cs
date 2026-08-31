using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.Product360View.Domain;

namespace NopStation.Plugin.Widgets.Product360View.Data.Builder;

public class ImageSettingBuilder : NopEntityBuilder<ProductImageSetting360>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		table.WithColumn("ProductId").AsInt32().ForeignKey<Product>();
	}
}
