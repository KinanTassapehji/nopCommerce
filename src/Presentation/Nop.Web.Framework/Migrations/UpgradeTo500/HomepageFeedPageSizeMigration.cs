using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-01 00:00:00", "5.00", UpdateMigrationType.Settings)]
public class HomepageFeedPageSizeMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();

        //the home page feed asks for a page at a time, and the product grid is 2 columns on mobile
        //and 4 on desktop - 8 fills whole rows on both
        var catalogSettings = settingService.LoadSetting<CatalogSettings>();
        catalogSettings.DefaultCategoryPageSize = 8;
        settingService.SaveSetting(catalogSettings);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}