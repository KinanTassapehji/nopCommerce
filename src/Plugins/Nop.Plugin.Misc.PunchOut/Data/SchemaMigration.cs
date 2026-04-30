using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.PunchOut.Domain;

namespace Nop.Plugin.Misc.PunchOut.Data;

[NopMigration("2026/04/23 09:09:17:6455442", "Misc.PunchOut.Data base schema", MigrationProcessType.Installation)]
public class SchemaMigration : Migration
{
    #region Methods

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        this.CreateTableIfNotExists<PunchOutLog>();
        this.CreateTableIfNotExists<PunchOutIdentity>();
    }

    /// <summary>
    /// Collects the DOWN migration expressions
    /// </summary>
    public override void Down()
    {
        this.DeleteTableIfExists<PunchOutLog>();
        this.DeleteTableIfExists<PunchOutIdentity>();
    }

    #endregion
}
