using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The name of the "Brand" icon (the store's own mark) in the icon pickers of Home page features and Welcome slides
/// </summary>
[NopUpdateMigration("2026-10-12 09:00:00", "5.00", UpdateMigrationType.Localization)]
public class BrandIconLocalizationMigration : MigrationBase
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
            ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Brand"] = "Brand mark"
        });

        //match on the language prefix - the packs ship as ar-SY and ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Enums.Nop.Core.Domain.Common.HomepageFeatureIcon.Brand"] = "شعار المتجر"
            }, arabic.Id);
    }

    /// <summary>Collects the DOWN migration expressions</summary>
    public override void Down()
    {
        //nothing to undo: a resource that stays is harmless
    }
}