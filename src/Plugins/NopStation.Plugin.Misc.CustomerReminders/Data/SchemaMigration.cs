using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Data;

[NopMigration("2026/01/23 22:40:00:0192456", "Updated Customer Reminders base schema", MigrationProcessType.Installation)]
public class SchemaMigration : Migration
{
	public override void Up()
	{
		if (!base.Schema.Table(NameCompatibilityManager.GetTableName(typeof(ReminderRule))).Exists())
		{
			base.Create.TableFor<ReminderRule>();
		}
		if (!base.Schema.Table(NameCompatibilityManager.GetTableName(typeof(Reminder))).Exists())
		{
			base.Create.TableFor<Reminder>();
		}
		if (!base.Schema.Table(NameCompatibilityManager.GetTableName(typeof(ReminderReport))).Exists())
		{
			base.Create.TableFor<ReminderReport>();
		}
		if (!base.Schema.Table(NameCompatibilityManager.GetTableName(typeof(ReminderExcludedCustomer))).Exists())
		{
			base.Create.TableFor<ReminderExcludedCustomer>();
		}
	}

	public override void Down()
	{
		string tableName = NameCompatibilityManager.GetTableName(typeof(ReminderExcludedCustomer));
		if (base.Schema.Table(tableName).Exists())
		{
			base.Delete.Table(tableName);
		}
		string tableName2 = NameCompatibilityManager.GetTableName(typeof(ReminderReport));
		if (base.Schema.Table(tableName2).Exists())
		{
			base.Delete.Table(tableName2);
		}
		string tableName3 = NameCompatibilityManager.GetTableName(typeof(Reminder));
		if (base.Schema.Table(tableName3).Exists())
		{
			base.Delete.Table(tableName3);
		}
		string tableName4 = NameCompatibilityManager.GetTableName(typeof(ReminderRule));
		if (base.Schema.Table(tableName4).Exists())
		{
			base.Delete.Table(tableName4);
		}
	}
}
