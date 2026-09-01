using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-01 00:00:02", "5.00", UpdateMigrationType.Localization)]
public class BestsellerLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //ponytail: match on the language prefix - the pack ships as ar-SY, older installs carry ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is null)
            return;

        //"الأكثر مبيعا" was missing its tanween
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Bestsellers"] = "الأكثر مبيعاً",
            ["Admin.Reports.Sales.Bestsellers"] = "الأكثر مبيعاً",
            ["Admin.Reports.Sales.Bestsellers.ByAmount"] = "الأكثر مبيعاً حسب الكمية"
        }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}