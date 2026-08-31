using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The product page now shares to WhatsApp/Facebook/Instagram and uses a quantity stepper,
/// replacing the ShareThis embed and the "email a friend" button.
/// </summary>
//00:00:01 on this date is taken by DiscountRuleLocalizationMigration - two migrations
//sharing a version means the second is silently recorded as applied and never runs
[NopUpdateMigration("2026-08-29 00:00:02", "5.00", UpdateMigrationType.Localization)]
public class ProductShareLocalizationMigration : MigrationBase
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
            ["Products.Share"] = "Share",
            ["Products.Share.LinkCopied"] = "Link copied",
            ["Products.Qty.Increase"] = "Increase quantity",
            ["Products.Qty.Decrease"] = "Decrease quantity"
        });

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Products.Share"] = "مشاركة",
                ["Products.Share.LinkCopied"] = "تم نسخ الرابط",
                ["Products.Qty.Increase"] = "زيادة الكمية",
                ["Products.Qty.Decrease"] = "إنقاص الكمية"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}