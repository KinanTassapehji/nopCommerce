using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The product page now folds its description, specifications and reviews into
/// panels and renders its own swipe gallery, so it needs a heading for the
/// description and names for the gallery controls.
/// </summary>
//00:00:02 on this date is taken by ProductShareLocalizationMigration - two migrations
//sharing a version means the second is silently recorded as applied and never runs
[NopUpdateMigration("2026-08-29 00:00:03", "5.00", UpdateMigrationType.Localization)]
public class ProductDetailsLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Products.Description"] = "Description",
            ["Products.Gallery.Label"] = "Product images",
            ["Products.Gallery.Position"] = "{0} of {1}",
            ["Products.Gallery.FullScreen"] = "View full screen"
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Products.Description"] = "الوصف",
                ["Products.Gallery.Label"] = "صور المنتج",
                ["Products.Gallery.Position"] = "{0} من {1}",
                ["Products.Gallery.FullScreen"] = "عرض بملء الشاشة"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}