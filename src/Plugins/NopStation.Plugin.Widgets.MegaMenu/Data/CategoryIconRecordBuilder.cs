using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.MegaMenu.Domains;

namespace NopStation.Plugin.Widgets.MegaMenu.Data;

public class CategoryIconRecordBuilder : NopEntityBuilder<CategoryIcon>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		table.WithColumn("CategoryId").AsInt32().WithColumn("PictureId")
			.AsInt32();
	}
}
