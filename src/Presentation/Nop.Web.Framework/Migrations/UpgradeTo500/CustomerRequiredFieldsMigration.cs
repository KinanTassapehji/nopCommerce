using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-21 00:00:01", "5.00", UpdateMigrationType.Settings)]
public class CustomerRequiredFieldsMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //CustomerPhoneMigration turned the phone field on but left it optional, so
        //that accounts predating it could still save. The store now needs a number
        //on every account to deliver against, so it becomes required - existing
        //customers fill it the next time they touch their account page.
        var customerSettings = settingService.LoadSetting<CustomerSettings>();
        customerSettings.PhoneRequired = true;
        settingService.SaveSetting(customerSettings);

        //Gender has no "required" setting of its own in nopCommerce: the validators
        //enforce it whenever the field is shown, so only the message is needed here.
        foreach (var language in languageService.GetAllLanguages(showHidden: true))
        {
            var arabic = language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Account.Fields.Gender.Required"] = arabic ? "الجنس مطلوب" : "Gender is required"
            }, language.Id);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
