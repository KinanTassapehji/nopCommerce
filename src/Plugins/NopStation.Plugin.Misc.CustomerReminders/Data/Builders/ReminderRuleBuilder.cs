using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Data.Builders;

public class ReminderRuleBuilder : NopEntityBuilder<ReminderRule>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		table.WithColumn("SystemName").AsString(100).NotNullable()
			.Unique()
			.WithColumn("Description")
			.AsString(500)
			.Nullable()
			.WithColumn("IsEnabled")
			.AsBoolean()
			.NotNullable()
			.WithColumn("AvailableTokens")
			.AsString(int.MaxValue)
			.Nullable()
			.WithColumn("RuleType")
			.AsString(100)
			.NotNullable()
			.WithColumn("CreatedOnUtc")
			.AsDateTime()
			.NotNullable()
			.WithColumn("Deleted")
			.AsBoolean()
			.NotNullable();
	}
}
