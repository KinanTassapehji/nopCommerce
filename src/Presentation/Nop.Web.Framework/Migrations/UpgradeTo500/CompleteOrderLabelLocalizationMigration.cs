using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The cart pill's Arabic label reads "أكمل الشراء" (complete the purchase) rather
/// than "أكمل الطلب" (complete the order).
/// </summary>
[NopUpdateMigration("2026-10-06 12:00:00", "5.00", UpdateMigrationType.Localization)]
public class CompleteOrderLabelLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //the store ships ar-SY, but match on the language rather than the
        //region so a store installed with any other Arabic culture is covered
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Mobile.Nav.CompleteOrder"] = "أكمل الشراء"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
