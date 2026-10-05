using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Payments.MtnCash.Domain;

namespace Nop.Plugin.Payments.MtnCash.Data;
[NopMigration("2025/12/18 12:00:00", "MtnCash plugin", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<MtnInvoice>();
    }
}
