using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-21 00:00:02", "5.00", UpdateMigrationType.Localization)]
public class AddressMapNoResultsLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //google finds nothing at all for a lot of local street names - the map search has to say so
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Address.Fields.MapLocation.NoResults"] = "No place found for this search. Try another name, or tap the map."
        });

        //ponytail: match on the language prefix - TmTm is ar-SY, Arabia is ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Address.Fields.MapLocation.NoResults"] = "لا توجد نتائج لهذا البحث. جرّب اسمًا آخر أو حدّد موقعك على الخريطة."
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}