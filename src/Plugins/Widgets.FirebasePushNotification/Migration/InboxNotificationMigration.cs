using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Widgets.FirebasePushNotification.Domain;

namespace Widgets.FirebasePushNotification.Migration;

//a schema migration runs at every startup, so the table reaches stores that installed the plugin
//before it existed as well as new installs; the Exists check makes either path a no-op the second time
[NopSchemaMigration("2026-10-04 15:30:00", "Widgets.FirebasePushNotification inbox", MigrationProcessType.NoMatter)]
public class InboxNotificationMigration : FluentMigrator.Migration
{
	public override void Up()
	{
		if (Schema.Table(nameof(InboxNotification)).Exists())
			return;

		Create.TableFor<InboxNotification>();
		Create.Index("IX_InboxNotification_CustomerId_IsRead").OnTable(nameof(InboxNotification))
			.OnColumn(nameof(InboxNotification.CustomerId)).Ascending()
			.OnColumn(nameof(InboxNotification.IsRead)).Ascending();
	}

	public override void Down()
	{
		if (Schema.Table(nameof(InboxNotification)).Exists())
			Delete.Table(nameof(InboxNotification));
	}
}
