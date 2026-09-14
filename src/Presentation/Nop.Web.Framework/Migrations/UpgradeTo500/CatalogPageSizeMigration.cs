using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Catalog;
using Nop.Services.Configuration;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Ten products a page on every storefront listing.
///
/// Two layers have to move together. The settings below are the default for a listing
/// that has no value of its own, but every seeded category and manufacturer carries its
/// own PageSize and PageSizeOptions, and the row wins - so the rows are rewritten too,
/// or the settings change nothing visible.
///
/// The options list has to contain the page size: with AllowCustomersToSelectPageSize
/// on, PreparePageSizeOptions snaps to the first option when the current size is not in
/// the list, so leaving "12, 24, 48" in place would serve 12 however the size is set.
///
/// DefaultCategoryPageSize moves too: HomeController feeds the home page "all products"
/// grid with it, and since every category row carries its own size, that setting governs
/// only the feed and brand-new categories. Ten fills whole rows there either way - the
/// grid is 5 across on desktop and 2 on mobile.
/// </summary>
[NopUpdateMigration("2026-09-14 00:00:03", "5.00", UpdateMigrationType.Settings)]
public class CatalogPageSizeMigration : MigrationBase
{
    protected const int PAGE_SIZE = 10;
    protected const string PAGE_SIZE_OPTIONS = "10, 20, 50";

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var categoryService = EngineContext.Current.Resolve<ICategoryService>();
        var manufacturerService = EngineContext.Current.Resolve<IManufacturerService>();

        var catalogSettings = settingService.LoadSetting<CatalogSettings>();
        catalogSettings.DefaultCategoryPageSize = PAGE_SIZE;
        catalogSettings.DefaultCategoryPageSizeOptions = PAGE_SIZE_OPTIONS;
        catalogSettings.DefaultManufacturerPageSize = PAGE_SIZE;
        catalogSettings.DefaultManufacturerPageSizeOptions = PAGE_SIZE_OPTIONS;
        catalogSettings.SearchPageProductsPerPage = PAGE_SIZE;
        catalogSettings.SearchPagePageSizeOptions = PAGE_SIZE_OPTIONS;
        catalogSettings.NewProductsPageSize = PAGE_SIZE;
        catalogSettings.NewProductsPageSizeOptions = PAGE_SIZE_OPTIONS;
        catalogSettings.ProductsByTagPageSize = PAGE_SIZE;
        catalogSettings.ProductsByTagPageSizeOptions = PAGE_SIZE_OPTIONS;
        settingService.SaveSetting(catalogSettings);

        foreach (var category in categoryService.GetAllCategoriesAsync(showHidden: true).Result)
        {
            category.PageSize = PAGE_SIZE;
            category.PageSizeOptions = PAGE_SIZE_OPTIONS;
            categoryService.UpdateCategoryAsync(category).Wait();
        }

        foreach (var manufacturer in manufacturerService.GetAllManufacturersAsync(showHidden: true).Result)
        {
            manufacturer.PageSize = PAGE_SIZE;
            manufacturer.PageSizeOptions = PAGE_SIZE_OPTIONS;
            manufacturerService.UpdateManufacturerAsync(manufacturer).Wait();
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}