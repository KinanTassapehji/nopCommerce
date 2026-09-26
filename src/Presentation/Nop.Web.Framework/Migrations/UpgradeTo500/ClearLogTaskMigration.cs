using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Keep the system log from filling the disk: the stock "Clear log" task ships disabled,
/// and its ClearLogOlderThanDays of 0 means "delete everything" rather than "keep N days",
/// so the retention has to be set before the task is switched on.
/// </summary>
[NopUpdateMigration("2026-09-26 12:00:01", "5.00", UpdateMigrationType.Settings)]
public class ClearLogTaskMigration : MigrationBase
{
    protected const int KEEP_DAYS = 30;
    protected const int RUN_EVERY_SECONDS = 24 * 60 * 60;

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var scheduleTaskService = EngineContext.Current.Resolve<IScheduleTaskService>();

        var commonSettings = settingService.LoadSetting<CommonSettings>();
        commonSettings.ClearLogOlderThanDays = KEEP_DAYS;
        settingService.SaveSetting(commonSettings);

        var task = scheduleTaskService.GetTaskByTypeAsync(typeof(ClearLogTask).FullName + ", Nop.Services").Result;
        if (task is null)
            return;

        task.Enabled = true;
        task.Seconds = RUN_EVERY_SECONDS;
        scheduleTaskService.UpdateTaskAsync(task).Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
