using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.Core.Domains;
using NopStation.Plugin.Misc.Core.Domains.SMS;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.Core.Data.Migrations;

[NopMigration("2026-02-07 00:00:00", "NopStation.Sms base schema create", MigrationProcessType.NoMatter)]
public class SchemaMigration : AutoReversingMigration
{
	public override void Up()
	{
		if (!base.Schema.Table<License>().Exists())
		{
			base.Create.TableFor<License>();
		}
		if (!base.Schema.Table<QueuedSms>().Exists())
		{
			base.Create.TableFor<QueuedSms>();
		}
		if (!base.Schema.Table<SmsTemplate>().Exists())
		{
			base.Create.TableFor<SmsTemplate>();
		}
	}
}
