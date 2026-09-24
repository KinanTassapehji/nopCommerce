using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-21 00:00:03", "5.00", UpdateMigrationType.Localization)]
public class AdminAddressLabelsLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //the admin kept the stock wording after the public store was renamed, so an order's
        //address read "العنوان 1" over a street and "المقاطعة / المنطقة" over a district.
        //Same words in both places - the staff and the customer are reading one address.
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.Address.Fields.Address1"] = "الشارع",
                ["Admin.Address.Fields.Address1.Hint"] = "أدخل الشارع.",
                ["Admin.Address.Fields.Address1.Required"] = "الشارع مطلوب",
                ["Admin.Address.Fields.County"] = "المنطقة",
                ["Admin.Address.Fields.County.Hint"] = "أدخل المنطقة.",
                ["Admin.Address.Fields.County.Required"] = "المنطقة مطلوبة.",
                //the order screen reads the address through its own resource family
                ["Admin.Orders.Address.Address1"] = "الشارع",
                ["Admin.Orders.Address.County"] = "المنطقة"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
