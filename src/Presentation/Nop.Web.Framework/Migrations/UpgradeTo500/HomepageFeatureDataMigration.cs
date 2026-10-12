using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The four trust strip items the home page view used to hard-code become rows of HomepageFeature,
/// in the same order and with the same icons. Their text is read from the Homepage.Usp.* resources
/// they used to come from, so wording an admin already changed there carries over: the Arabic text
/// is the item's own value (Arabic is the store's language), English its localized value.
/// </summary>
[NopUpdateMigration("2026-10-03 18:00:01", "5.00", UpdateMigrationType.Data)]
public class HomepageFeatureDataMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var repository = EngineContext.Current.Resolve<IRepository<HomepageFeature>>();
        if (repository.Table.Any())
            return;

        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var localizedEntityService = EngineContext.Current.Resolve<ILocalizedEntityService>();
        var languages = EngineContext.Current.Resolve<ILanguageService>().GetAllLanguages(showHidden: true);
        var arabic = languages.FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        var english = languages.FirstOrDefault(language => language.LanguageCulture.StartsWith("en", StringComparison.OrdinalIgnoreCase));

        string text(string key, Nop.Core.Domain.Localization.Language language) => language is null
            ? null
            : localizationService.GetResourceAsync(key, language.Id, false, string.Empty, true).Result;

        var items = new (string Key, HomepageFeatureIcon Icon)[]
        {
            ("Homepage.Usp.Delivery", HomepageFeatureIcon.Delivery),
            ("Homepage.Usp.Cod", HomepageFeatureIcon.CashOnDelivery),
            ("Homepage.Usp.Genuine", HomepageFeatureIcon.Genuine),
            ("Homepage.Usp.Support", HomepageFeatureIcon.Support)
        };

        for (var i = 0; i < items.Length; i++)
        {
            var (key, icon) = items[i];
            var main = arabic ?? english;
            var feature = new HomepageFeature
            {
                Title = text(key, main),
                Hint = text($"{key}.Hint", main),
                Icon = icon,
                Published = true,
                DisplayOrder = i + 1
            };
            repository.InsertAsync(feature, false).Wait();

            if (english is null || english == main)
                continue;

            localizedEntityService.SaveLocalizedValueAsync(feature, f => f.Title, text(key, english), english.Id).Wait();
            localizedEntityService.SaveLocalizedValueAsync(feature, f => f.Hint, text($"{key}.Hint", english), english.Id).Wait();
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
