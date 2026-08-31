using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.Core.Domains;

namespace NopStation.Plugin.Misc.Core.Data.Builders;

public class LicenseBuilder : NopEntityBuilder<License>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		table.WithColumn("Key").AsString(int.MaxValue);
	}
}
