using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Data.Builders;

public class SmsTemplateRecordBuilder : NopEntityBuilder<SmsTemplate>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		table.WithColumn("Name").AsString(200).NotNullable()
			.WithColumn("Body")
			.AsString(int.MaxValue)
			.NotNullable()
			.WithColumn("Active")
			.AsBoolean()
			.NotNullable()
			.WithColumn("LimitedToStores")
			.AsBoolean()
			.NotNullable()
			.WithColumn("SubjectToAcl")
			.AsBoolean()
			.NotNullable()
			.WithColumn("ProviderSystemName")
			.AsString(100)
			.Nullable();
	}
}
