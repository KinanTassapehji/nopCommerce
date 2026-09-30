using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The Arabic "added to the cart" message read "سلةا لتسوق" (misplaced space) with a double space
/// before the link; also "تمت" for the feminine إضافة
/// </summary>
[NopUpdateMigration("2026-09-30 17:00:00", "5.00", UpdateMigrationType.Localization)]
public class AddedToCartArabicFixMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Products.ProductHasBeenAddedToTheCart.Link"] = "تمت إضافة المنتج إلى <a href=\"{0}\">سلة التسوق</a>"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}