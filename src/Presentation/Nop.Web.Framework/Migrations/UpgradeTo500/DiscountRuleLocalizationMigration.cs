using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-08-29 00:00:01", "5.00", UpdateMigrationType.Localization)]
public class DiscountRuleLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //ponytail: English names already come from plugin.json, so only the Arabic override is stored
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar"));
        if (arabic is null)
            return;

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Plugins.FriendlyName.DiscountRequirement.MustBeAssignedToCustomerRole"] = "يجب أن يكون العميل ضمن دور محدد",
            ["Plugins.FriendlyName.NopStation.Plugin.DiscountRules.CartTotal"] = "الحد الأدنى لإجمالي سلة التسوق",
            ["Plugins.FriendlyName.NopStation.Plugin.DiscountRules.CustomerBirthday"] = "خصم في عيد ميلاد العميل",
            ["Plugins.FriendlyName.NopStation.Plugin.DiscountRules.CustomerGender"] = "جنس العميل",
            ["Plugins.FriendlyName.NopStation.Plugin.DiscountRules.DaysOfWeek"] = "أيام الأسبوع",
            ["Plugins.FriendlyName.NopStation.Plugin.DiscountRules.OrderRange"] = "نطاق مبلغ الطلب",
            ["Plugins.FriendlyName.NopStation.Plugin.DiscountRules.TimeOfDay"] = "وقت من اليوم",
            ["Plugins.FriendlyName.NopStation.Plugin.DiscountRules.TotalSpent"] = "إجمالي المبلغ المنفق"
        }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}