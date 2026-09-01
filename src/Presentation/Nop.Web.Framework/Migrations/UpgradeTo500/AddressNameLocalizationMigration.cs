using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-01 00:00:04", "5.00", UpdateMigrationType.Localization)]
public class AddressNameLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //customers name their own addresses instead of retyping their name and email for each one
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Address.Fields.AddressName"] = "Address name",
            ["Address.Fields.AddressName.Placeholder"] = "Home, Work, ..."
        });

        //ponytail: match on the language prefix - the pack ships as ar-SY, older installs carry ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Address.Fields.AddressName"] = "اسم العنوان",
                ["Address.Fields.AddressName.Placeholder"] = "المنزل، العمل، ..."
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}