using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo500;

/// <summary>
/// The home page trust strip becomes a list the admin manages (Content management > Home page features)
/// </summary>
[NopSchemaMigration("2026-10-03 18:00:00", "SchemaMigration for 5.00.0 - home page features")]
public class HomepageFeatureMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(HomepageFeature)).Exists())
            Create.TableFor<HomepageFeature>();
    }
}
