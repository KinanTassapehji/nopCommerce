using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Stores;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Data.Builders;

public class ReminderBuilder : NopEntityBuilder<Reminder>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		table.WithColumn("Name").AsString(400).NotNullable()
			.WithColumn("IsEnabled")
			.AsBoolean()
			.NotNullable()
			.WithColumn("MessageTemplateId")
			.AsInt32()
			.NotNullable()
			.WithColumn("StoreId")
			.AsInt32()
			.ForeignKey<Store>()
			.OnDelete(Rule.Cascade)
			.WithColumn("ReminderRuleId")
			.AsInt32()
			.ForeignKey<ReminderRule>()
			.OnDelete(Rule.Cascade)
			.WithColumn("VendorId")
			.AsInt32()
			.NotNullable()
			.WithColumn("DateGreaterThanIntervalTypeId")
			.AsInt32()
			.NotNullable()
			.WithColumn("DateGreaterThan")
			.AsInt32()
			.NotNullable()
			.WithColumn("DateLowerThanIntervalTypeId")
			.AsInt32()
			.NotNullable()
			.WithColumn("DateLowerThan")
			.AsInt32()
			.NotNullable()
			.WithColumn("IntervalBetweenMessagesTypeId")
			.AsInt32()
			.NotNullable()
			.WithColumn("IntervalBetweenMessages")
			.AsInt32()
			.NotNullable()
			.WithColumn("MaxMessagesPerCustomer")
			.AsInt32()
			.NotNullable()
			.WithColumn("ExecutedOnUtc")
			.AsDateTime()
			.NotNullable()
			.WithColumn("Deleted")
			.AsBoolean()
			.NotNullable();
	}
}
