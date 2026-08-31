using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Data.Builders;

public class ReminderExcludedCustomerBuilder : NopEntityBuilder<ReminderExcludedCustomer>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		table.WithColumn("ReminderId").AsInt32().ForeignKey<Reminder>()
			.OnDelete(Rule.Cascade)
			.WithColumn("CustomerId")
			.AsInt32()
			.ForeignKey<Customer>()
			.OnDelete(Rule.Cascade);
	}
}
