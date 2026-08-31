using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Widgets.FirebasePushNotification.Domain;

namespace Widgets.FirebasePushNotification.Data;

public class FirebaseDeviceTokenBuilder : NopEntityBuilder<FirebaseDeviceToken>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		table.WithColumn("Token").AsString(1000).NotNullable()
			.WithColumn("Platform")
			.AsString(32)
			.NotNullable()
			.WithColumn("IsActive")
			.AsBoolean()
			.NotNullable()
			.WithColumn("CreatedOnUtc")
			.AsDateTime2()
			.NotNullable()
			.WithColumn("UpdatedOnUtc")
			.AsDateTime2()
			.NotNullable()
			.WithColumn("LastUsedOnUtc")
			.AsDateTime2()
			.Nullable();
	}
}
