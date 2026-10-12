using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Directory;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The Syrian pound's custom formatting is "#,##0 'ل.س'", and it was one format for every
/// language, so the English store printed the Arabic symbol too. PriceFormatter now reads the
/// formatting translated for the working language; English gets the ISO code. Only when no
/// English formatting was entered yet, so an admin's own choice survives.
/// </summary>
[NopUpdateMigration("2026-09-29 18:00:00", "5.00", UpdateMigrationType.Data)]
public class CurrencyFormattingLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var currencyService = EngineContext.Current.Resolve<ICurrencyService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var localizedEntityService = EngineContext.Current.Resolve<ILocalizedEntityService>();

        if (currencyService.GetCurrencyByCodeAsync("SYP").Result is not { } currency)
            return;

        var english = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("en", StringComparison.OrdinalIgnoreCase));
        if (english is null)
            return;

        var formatting = localizationService.GetLocalizedAsync(currency, entity => entity.CustomFormatting, english.Id, returnDefaultValue: false).Result;
        if (!string.IsNullOrEmpty(formatting))
            return;

        localizedEntityService.SaveLocalizedValueAsync(currency, entity => entity.CustomFormatting, "#,##0 'SYP'", english.Id).Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}