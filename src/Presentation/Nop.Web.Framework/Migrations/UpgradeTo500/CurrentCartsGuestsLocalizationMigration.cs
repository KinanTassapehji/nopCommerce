using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-22 12:00:00", "5.00", UpdateMigrationType.Localization)]
public class CurrentCartsGuestsLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //the "include guests" checkbox on the current carts page
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.ShoppingCartType.IncludeGuests"] = "Include guests",
            ["Admin.ShoppingCartType.IncludeGuests.Hint"] = "Check to also list carts of customers who are not registered."
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.ShoppingCartType.IncludeGuests"] = "تضمين الزوار",
                ["Admin.ShoppingCartType.IncludeGuests.Hint"] = "حدد لعرض سلال الزوار غير المسجلين أيضاً."
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}