using FluentMigrator;
using Nop.Core.Domain.Common;

namespace Nop.Data.Migrations.UpgradeTo500;

/// <summary>
/// Adds the customer-facing name of an address ("Home", "Work", ...)
/// </summary>
[NopSchemaMigration("2026-09-01 00:00:03", "SchemaMigration for 5.00.0 - address name")]
public class AddressNameMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(Address)).Column(nameof(Address.AddressName)).Exists())
            Alter.Table(nameof(Address)).AddColumn(nameof(Address.AddressName)).AsString(400).Nullable();
    }
}