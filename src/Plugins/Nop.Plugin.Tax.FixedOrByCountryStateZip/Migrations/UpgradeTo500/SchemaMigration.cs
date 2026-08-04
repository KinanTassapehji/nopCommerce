using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Tax.FixedOrByCountryStateZip.Domain;

namespace Nop.Plugin.Tax.FixedOrByCountryStateZip.Migrations.UpgradeTo500;

[NopSchemaMigration("2026-08-14 12:00:00", "Tax.FixedOrByCountryStateZip 5.0.5. Schema Migration")]
public class SchemaMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        this.AddOrAlterColumnFor<TaxRate>(t => t.Percentage2)
            .AsDecimal(18, 4)
            .Nullable();
        this.AddOrAlterColumnFor<TaxRate>(t => t.Percentage3)
            .AsDecimal(18, 4)
            .Nullable();
    }
}
