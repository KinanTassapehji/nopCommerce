using System.Text.RegularExpressions;
using FluentMigrator;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-24 12:00:00", "5.00", UpdateMigrationType.Localization)]
public class ArabicHamzaLocalizationMigration : MigrationBase
{
    //Words written with a bare alef that take a hamza below: form IV verbal nouns (إدارة، إضافة،
    //إرسال...), إلى, and a few loanwords. Hamzat al-wasl words (استخدام، اختيار، انتهاء) are correct
    //bare and are deliberately absent. The language pack carries the same fix for new installs;
    //this rewrites what is already in the database - including plugin text that exists only there.
    //ponytail: a fixed word list, not a rule - a new misspelling needs adding here
    private static readonly string[] _stems =
    [
        "ادارة", "اضافة", "اضافات", "اضافتها", "اضافه", "اضافية", "ادخال", "انشاء", "اعداد", "اعدادات",
        "الكتروني", "الغاء", "ارسال", "ارســـال", "اجمالي", "اجمالى", "ازالة", "اشعار", "اشعارات", "اصدار",
        "اصدارات", "اعادة", "اخفاء", "اظهار", "اتاحة", "انهاء", "اخراج", "امكانية", "ارفاق", "اجبارية",
        "اغلاق", "اخطار", "انقاص", "اشارة", "اقفال", "امساك", "انجليزية", "ايميل", "اكسيل", "انستجرام", "اعلام"
    ];

    //a whole word only: an optional و/ف, then بال/كال/لال/ال/لل/ب/ك/ل, then the stem; إلى takes و/ف only
    private static readonly Regex _bareAlef = new(
        "(?<![ء-ي])([وف]?(?:[بكل]?ال|لل|[بكل])?)(" + string.Join("|", _stems) + ")(?![ء-ي])" +
        "|(?<![ء-ي])([وف]?)(الى)(?![ء-ي])");

    private static string Fix(string text) => _bareAlef.Replace(text, match => match.Groups[2].Success
        ? match.Groups[1].Value + "إ" + match.Groups[2].Value[1..]
        : match.Groups[3].Value + "إ" + match.Groups[4].Value[1..]);

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var languageService = EngineContext.Current.Resolve<ILanguageService>();
        var resourceRepository = EngineContext.Current.Resolve<IRepository<LocaleStringResource>>();

        foreach (var arabic in languageService.GetAllLanguages(showHidden: true)
            .Where(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase)))
        {
            var changed = resourceRepository.Table.Where(resource => resource.LanguageId == arabic.Id).ToList()
                .Where(resource => resource.ResourceValue is not null && _bareAlef.IsMatch(resource.ResourceValue))
                .ToList();

            foreach (var resource in changed)
                resource.ResourceValue = Fix(resource.ResourceValue);

            if (changed.Count > 0)
                resourceRepository.Update(changed);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}