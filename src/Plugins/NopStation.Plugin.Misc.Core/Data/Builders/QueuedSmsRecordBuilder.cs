using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Data.Builders;

public class QueuedSmsRecordBuilder : NopEntityBuilder<QueuedSms>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		table.WithColumn("CustomerId").AsInt32().Nullable()
			.WithColumn("StoreId")
			.AsInt32()
			.NotNullable()
			.WithColumn("Body")
			.AsString(int.MaxValue)
			.NotNullable()
			.WithColumn("PhoneNumber")
			.AsString(50)
			.NotNullable()
			.WithColumn("SentTries")
			.AsInt32()
			.NotNullable()
			.WithColumn("Error")
			.AsString(int.MaxValue)
			.Nullable()
			.WithColumn("CreatedOnUtc")
			.AsDateTime()
			.NotNullable()
			.WithColumn("SentOnUtc")
			.AsDateTime()
			.Nullable()
			.WithColumn("ProviderSystemName")
			.AsString(100)
			.Nullable()
			.WithColumn("ExternalMessageId")
			.AsString(255)
			.Nullable();
	}
}
