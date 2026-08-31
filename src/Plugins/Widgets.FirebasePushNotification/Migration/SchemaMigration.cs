using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Widgets.FirebasePushNotification.Domain;

namespace Widgets.FirebasePushNotification.Migration;

[NopMigration("2026-02-10 00:00:00", "Widgets.FirebasePushNotification schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
	public override void Up()
	{
		if (!base.Schema.Table("FirebaseDeviceToken").Exists())
		{
			base.Create.TableFor<FirebaseDeviceToken>();
			base.Create.Index("IX_FirebaseDeviceToken_CustomerId").OnTable("FirebaseDeviceToken").OnColumn("CustomerId")
				.Ascending();
			base.Create.Index("IX_FirebaseDeviceToken_Token").OnTable("FirebaseDeviceToken").OnColumn("Token")
				.Ascending();
			base.Create.Index("UX_FirebaseDeviceToken_CustomerId_Token").OnTable("FirebaseDeviceToken").OnColumn("CustomerId")
				.Ascending()
				.OnColumn("Token")
				.Ascending()
				.WithOptions()
				.Unique();
		}
	}
}
