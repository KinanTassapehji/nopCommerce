using FluentMigrator;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;
using Nop.Services.Security;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Email accounts, stores, countries and the widget/plugin list pages become super-administrator only.
/// The widget and plugin permissions themselves stay with administrators: the home slider settings
/// and the third-party plugin menus depend on them.
/// </summary>
[NopUpdateMigration("2026-09-26 14:00:00", "5.00", UpdateMigrationType.Data)]
public class SuperAdministratorsConfigurationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        SuperAdministratorsMigration.MoveToSuperAdministrators(EngineContext.Current.Resolve<INopDataProvider>(),
            StandardPermission.Configuration.MANAGE_EMAIL_ACCOUNTS,
            StandardPermission.Configuration.MANAGE_STORES,
            StandardPermission.Configuration.MANAGE_COUNTRIES,
            StandardPermission.Configuration.MANAGE_PLUGIN_AND_WIDGET_LISTS);

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Security.Permission.Configuration.ManagePluginAndWidgetLists"] = "Manage the widget and plugin lists"
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Security.Permission.Configuration.ManagePluginAndWidgetLists"] = "إدارة قوائم عناصر الواجهة والإضافات"
            }, arabic.Id);

        //permission mappings were written past the services, so drop whatever start-up already cached
        EngineContext.Current.Resolve<IStaticCacheManager>().ClearAsync().Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}