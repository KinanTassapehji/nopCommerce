using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-19 00:00:02", "5.00", UpdateMigrationType.Localization)]
public class AddressMapLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //the address form carries a google map, so the customer drops a pin instead of describing where they live
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Address.Fields.MapLocation.Search"] = "Search for an area or a street",
            ["Address.Fields.MapLocation.UseMyLocation"] = "Use my location",
            ["Address.Fields.MapLocation.Hint"] = "Tap the map or drag the pin to your exact location.",
            ["Address.Fields.MapLocation.EditManually"] = "Edit the address manually"
        });

        //ponytail: match on the language prefix - the pack ships as ar-SY, older installs carry ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Address.Fields.MapLocation.Search"] = "ابحث عن منطقة أو شارع",
                ["Address.Fields.MapLocation.UseMyLocation"] = "حدد موقعي",
                ["Address.Fields.MapLocation.Hint"] = "اضغط على الخريطة أو حرّك المؤشر إلى موقعك بالضبط.",
                ["Address.Fields.MapLocation.EditManually"] = "تعديل العنوان يدويًا"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}