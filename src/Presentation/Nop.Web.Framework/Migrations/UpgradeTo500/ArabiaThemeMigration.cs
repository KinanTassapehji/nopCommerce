using FluentMigrator;
using Nop.Core.Domain;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-10 00:00:00", "5.00", UpdateMigrationType.Settings)]
public class ArabiaThemeMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();

        //the theme directory was renamed TmTm -> Arabia; an installed store still points at the old
        //name, and ThemeProvider falls back to nothing, so the storefront loses its whole brand layer
        var storeInformationSettings = settingService.LoadSetting<StoreInformationSettings>();
        if (!string.Equals(storeInformationSettings.DefaultStoreTheme, "Arabia", StringComparison.OrdinalIgnoreCase))
        {
            storeInformationSettings.DefaultStoreTheme = "Arabia";
            settingService.SaveSetting(storeInformationSettings);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}