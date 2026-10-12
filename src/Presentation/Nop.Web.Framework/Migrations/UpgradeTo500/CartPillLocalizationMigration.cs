using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Label of the cart pill: while the cart holds items, the mobile cart tab and the
/// header cart link turn into a "complete order" pill. The stock "Checkout.Button"
/// reads "الدفع" (payment), which is not what the pill offers.
/// </summary>
[NopUpdateMigration("2026-09-29 14:00:00", "5.00", UpdateMigrationType.Localization)]
public class CartPillLocalizationMigration : MigrationBase
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
            ["Mobile.Nav.CompleteOrder"] = "Complete order"
        });

        //the store ships ar-SY, but match on the language rather than the
        //region so a store installed with any other Arabic culture is covered
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Mobile.Nav.CompleteOrder"] = "أكمل الطلب"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}