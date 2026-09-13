using FluentMigrator;
using Nop.Core.Domain.Menus;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;
using Nop.Services.Menus;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-12 00:00:02", "5.00", UpdateMigrationType.Data)]
public class PageLinksMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();
        var menuService = EngineContext.Current.Resolve<IMenuService>();

        //accessible name for the two page-link rows - the header's and the footer's
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Nav.Information"] = "Information"
        });

        //ponytail: match on the language prefix, not the exact culture - the pack ships ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Nav.Information"] = "معلومات"
            }, arabic.Id);

        //The theme flattens the footer menus into one slim row of links, so the two
        //columns that only repeated the header and the account pages would land in it
        //as duplicates. Unpublished rather than deleted: republish from admin to get
        //them back, and the items are still there to copy into Information.
        var duplicateMenus = new[] { "Customer service", "My account" };
        foreach (var menu in menuService.GetAllMenusAsync(MenuType.Footer, showHidden: true).Result
                     .Where(menu => duplicateMenus.Contains(menu.Name) && menu.Published))
        {
            menu.Published = false;
            menuService.UpdateMenuAsync(menu).Wait();
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
