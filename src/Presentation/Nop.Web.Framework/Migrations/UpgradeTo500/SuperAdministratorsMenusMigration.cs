using FluentMigrator;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Security;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Content management > Menus becomes super-administrator only
/// </summary>
[NopUpdateMigration("2026-09-26 23:41:00", "5.00", UpdateMigrationType.Data)]
public class SuperAdministratorsMenusMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        SuperAdministratorsMigration.MoveToSuperAdministrators(EngineContext.Current.Resolve<INopDataProvider>(),
            StandardPermission.ContentManagement.MENU_VIEW,
            StandardPermission.ContentManagement.MENU_CREATE_EDIT_DELETE);

        //permission mappings were written past the services, so drop whatever start-up already cached
        EngineContext.Current.Resolve<IStaticCacheManager>().ClearAsync().Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}