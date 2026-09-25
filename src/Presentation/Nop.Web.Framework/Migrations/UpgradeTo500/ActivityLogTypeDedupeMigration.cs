using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The 4.90 data migration re-inserted PublicStore.SuccessfulLogin on every run (guarded now),
/// leaving dozens of identical rows on the activity types page. Keep the oldest row per
/// keyword, point any log entries at it, drop the rest.
/// </summary>
[NopUpdateMigration("2026-09-25 19:00:00", "5.00", UpdateMigrationType.Data)]
public class ActivityLogTypeDedupeMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var dataProvider = EngineContext.Current.Resolve<INopDataProvider>();
        var types = dataProvider.GetTable<Nop.Core.Domain.Logging.ActivityLogType>().ToList();
        var duplicates = types
            .GroupBy(type => type.SystemKeyword, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1);

        foreach (var group in duplicates)
        {
            var keep = group.Min(type => type.Id);
            foreach (var extra in group.Where(type => type.Id != keep))
            {
                dataProvider.ExecuteNonQueryAsync($"UPDATE ActivityLog SET ActivityLogTypeId = {keep} WHERE ActivityLogTypeId = {extra.Id}").Wait();
                dataProvider.DeleteEntity(extra);
            }
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}