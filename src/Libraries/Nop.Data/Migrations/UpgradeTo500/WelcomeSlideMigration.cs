using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Data.Extensions;

namespace Nop.Data.Migrations.UpgradeTo500;

/// <summary>
/// The welcome tour shown on a visitor's first visit becomes a list the admin manages (Content management > Welcome slides)
/// </summary>
[NopSchemaMigration("2026-10-11 12:00:00", "SchemaMigration for 5.00.0 - welcome slides")]
public class WelcomeSlideMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(WelcomeSlide)).Exists())
            Create.TableFor<WelcomeSlide>();
    }
}