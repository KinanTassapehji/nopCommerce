using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Data.Extensions;
using Nop.Data.Migrations;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The maintenance request form used to be e-mail only; the requests now land in a table
/// so the admin can work through them. Nothing backfills the ones already mailed.
/// </summary>
[NopSchemaMigration("2026-09-22 00:00:00", "Maintenance request table")]
public class MaintenanceRequestTableMigration : ForwardOnlyMigration
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(MaintenanceRequest)).Exists())
            Create.TableFor<MaintenanceRequest>();
    }
}