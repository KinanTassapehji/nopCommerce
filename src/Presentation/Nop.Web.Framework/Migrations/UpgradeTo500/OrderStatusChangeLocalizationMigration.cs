using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-20 00:00:03", "5.00", UpdateMigrationType.Localization)]
public class OrderStatusChangeLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //TmTm is ar-SY and Arabia is ar-SA, so match the language, not the country
        foreach (var arabic in languageService.GetAllLanguages(showHidden: true)
                     .Where(language => language.LanguageCulture.StartsWith("ar")))
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                //"تغيير الوضع" sits next to the status with no hint of which status it means
                ["Admin.Orders.Fields.OrderStatus.Change"] = "تغيير حالة الطلب"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}