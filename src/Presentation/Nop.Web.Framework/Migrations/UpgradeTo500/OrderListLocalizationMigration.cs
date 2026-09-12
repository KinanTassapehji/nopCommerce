using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

//2026-09-01 00:00:02 was already taken by BestsellerLocalizationMigration in this same
//assembly. FluentMigrator turns the timestamp into the version number, so two of them
//is a DuplicateMigrationException the moment IVersionLoader is resolved - the app will
//not boot at all, and on a store that somehow got past it only one of the two ever ran.
[NopUpdateMigration("2026-09-01 00:00:06", "5.00", UpdateMigrationType.Localization)]
public class OrderListLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //ponytail: match on the language prefix, not the exact culture - the pack ships ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is null)
            return;

        //the order list search panel kept the English labels
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.Orders.List.BillingPhone"] = "رقم الهاتف",
            ["Admin.Orders.List.BillingPhone.Hint"] = "تصفية حسب رقم هاتف العميل.",
            ["Admin.Orders.List.BillingEmail"] = "البريد الإلكتروني",
            ["Admin.Orders.List.BillingEmail.Hint"] = "تصفية حسب البريد الإلكتروني للعميل.",
            ["Admin.Orders.List.BillingLastName"] = "اسم العائلة",
            ["Admin.Orders.List.BillingLastName.Hint"] = "تصفية حسب اسم عائلة العميل."
        }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}