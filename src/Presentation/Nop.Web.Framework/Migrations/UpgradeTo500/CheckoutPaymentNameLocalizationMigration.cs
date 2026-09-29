using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-29 12:00:00", "5.00", UpdateMigrationType.Localization)]
public class CheckoutPaymentNameLocalizationMigration : MigrationBase
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

        //the checkout's order details step names the payment method under the total, and the plugin
        //only shipped its English friendly name
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Plugins.FriendlyName.Payments.CashOnDelivery"] = "الدفع عند الاستلام"
        }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}