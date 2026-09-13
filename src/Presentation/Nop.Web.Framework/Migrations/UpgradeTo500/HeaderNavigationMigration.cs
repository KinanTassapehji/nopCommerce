using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Menus;
using Nop.Core.Domain.Topics;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Menus;
using Nop.Services.Topics;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-12 00:00:03", "5.00", UpdateMigrationType.Data)]
public class HeaderNavigationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();
        var manufacturerService = EngineContext.Current.Resolve<IManufacturerService>();
        var menuService = EngineContext.Current.Resolve<IMenuService>();
        var topicService = EngineContext.Current.Resolve<ITopicService>();

        //the two browse dropdowns beside the logo
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Nav.Products"] = "Products",
            ["Nav.Brands"] = "Brands"
        });

        //ponytail: match on the language prefix, not the exact culture - the pack ships ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Nav.Products"] = "المنتجات",
                ["Nav.Brands"] = "العلامات التجارية"
            }, arabic.Id);

        //All eight brands belong on the strip and on /manufacturer/all, whether or
        //not stock has been loaded against them yet - the row is the company's
        //brand portfolio, not an in-stock filter. This reverses the empty-brand
        //unpublish BrandStripMigration used to do.
        foreach (var manufacturer in manufacturerService.GetAllManufacturersAsync(showHidden: true).Result
                     .Where(manufacturer => !manufacturer.Published))
        {
            manufacturer.Published = true;
            manufacturerService.UpdateManufacturerAsync(manufacturer).Wait();
        }

        //The footer is one slim row: the sitemap and the three policy topics are the
        //boilerplate half of it. Unpublished rather than deleted - the pages stay
        //published and reachable by URL, and republishing the item from admin puts
        //the link straight back.
        var boilerplate = new[] { "ShippingInfo", "PrivacyInfo", "ConditionsOfUse" }
            .Select(systemName => topicService.GetTopicBySystemNameAsync(systemName).Result?.Id)
            .Where(id => id.HasValue)
            .ToList();

        foreach (var menu in menuService.GetAllMenusAsync(MenuType.Footer, showHidden: true).Result)
            foreach (var item in menuService.GetAllMenuItemsAsync(menu.Id, showHidden: true).Result
                         .Where(item => item.Published && IsBoilerplate(item)))
            {
                item.Published = false;
                menuService.UpdateMenuItemAsync(item).Wait();
            }

        bool IsBoilerplate(MenuItem item) =>
            item.MenuItemType == MenuItemType.TopicPage
                ? boilerplate.Contains(item.EntityId)
                : string.Equals(item.RouteName, Nop.Core.Http.NopRouteNames.General.SITEMAP, StringComparison.OrdinalIgnoreCase);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
