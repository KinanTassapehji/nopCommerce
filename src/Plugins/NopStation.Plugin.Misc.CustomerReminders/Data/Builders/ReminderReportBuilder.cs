using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Data.Builders;

public class ReminderReportBuilder : NopEntityBuilder<ReminderReport>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		table.WithColumn("ReminderId").AsInt32().Nullable()
			.WithColumn("ReminderName")
			.AsString(400)
			.NotNullable()
			.WithColumn("CustomerId")
			.AsInt32()
			.Nullable()
			.WithColumn("CustomerName")
			.AsString(400)
			.NotNullable()
			.WithColumn("CustomerEmail")
			.AsString(400)
			.NotNullable()
			.WithColumn("StoreId")
			.AsInt32()
			.NotNullable()
			.WithColumn("StoreName")
			.AsString(400)
			.NotNullable()
			.WithColumn("CreatedOnUtc")
			.AsDateTime()
			.NotNullable()
			.WithColumn("IsMessageSent")
			.AsBoolean()
			.NotNullable();
	}
}
