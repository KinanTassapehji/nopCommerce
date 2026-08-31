using FluentMigrator;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.Core.Domains;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.Core.Data.Migrations.UpgradeTo46013;

[NopMigration("2023-05-15 11:00:00", "NopStation.Core change license key length update", MigrationProcessType.Update)]
public class LicenseKeyMigration : MigrationBase
{
	public override void Up()
	{
		if (!base.Schema.TableColumn((License x) => x.Key).Exists())
		{
			base.Alter.Table<License>().AlterColumn("Key").AsString(int.MaxValue);
		}
	}

	public override void Down()
	{
	}
}
