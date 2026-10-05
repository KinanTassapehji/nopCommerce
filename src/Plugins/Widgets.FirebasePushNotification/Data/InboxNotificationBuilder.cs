using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Widgets.FirebasePushNotification.Domain;

namespace Widgets.FirebasePushNotification.Data;

public class InboxNotificationBuilder : NopEntityBuilder<InboxNotification>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		table.WithColumn(nameof(InboxNotification.Title)).AsString(400).NotNullable()
			.WithColumn(nameof(InboxNotification.Body)).AsString(2000).NotNullable()
			.WithColumn(nameof(InboxNotification.Link)).AsString(1000).Nullable();
	}
}
