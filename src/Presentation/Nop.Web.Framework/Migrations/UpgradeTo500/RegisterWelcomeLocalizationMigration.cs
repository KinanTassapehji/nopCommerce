using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The welcome page after registering, and the phone tab bar's "Products" tab
/// </summary>
[NopUpdateMigration("2026-10-04 13:00:00", "5.00", UpdateMigrationType.Localization)]
public class RegisterWelcomeLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //English first, for every language; then Arabic for its own language only
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Account.Register.Welcome.Title"] = "Welcome to {0}!",
            ["Account.Register.Welcome.TitleNamed"] = "Welcome to {0}, {1}!",
            ["Account.Register.Welcome.Pending"] = "One more step",
            ["Account.Register.Welcome.Perk.Orders"] = "Track your orders and their history any time",
            ["Account.Register.Welcome.Perk.Addresses"] = "Save your addresses for a faster checkout",
            ["Account.Register.Welcome.Perk.Cart"] = "Your cart is kept for you on every device",
            ["Account.Register.Welcome.Shop"] = "Start shopping",
            ["Mobile.Nav.Products"] = "Products"
        });

        //ponytail: match on the language prefix - the packs ship as ar-SY and ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Account.Register.Welcome.Title"] = "أهلاً بك في {0}!",
                ["Account.Register.Welcome.TitleNamed"] = "أهلاً بك في {0}، {1}!",
                ["Account.Register.Welcome.Pending"] = "خطوة أخيرة",
                ["Account.Register.Welcome.Perk.Orders"] = "تابع طلباتك وسجلّها في أي وقت",
                ["Account.Register.Welcome.Perk.Addresses"] = "احفظ عناوينك لإتمام الطلب بخطوات أقل",
                ["Account.Register.Welcome.Perk.Cart"] = "سلّتك محفوظة لك على كل أجهزتك",
                ["Account.Register.Welcome.Shop"] = "ابدأ التسوق",
                ["Mobile.Nav.Products"] = "المنتجات"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
