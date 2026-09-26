using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Installation;
using Nop.Services.Localization;
using Nop.Services.Topics;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The conditions-of-use topic shipped as a one-line "write your terms here"
/// placeholder. Fills it with the real Arabic terms and their English localized value.
/// Only the untouched placeholder is replaced, so terms an admin has already written survive.
/// </summary>
[NopUpdateMigration("2026-09-25 20:00:00", "5.00", UpdateMigrationType.Data)]
public class ConditionsOfUseTopicMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var languageService = EngineContext.Current.Resolve<ILanguageService>();
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var localizedEntityService = EngineContext.Current.Resolve<ILocalizedEntityService>();
        var topicService = EngineContext.Current.Resolve<ITopicService>();

        if (topicService.GetTopicBySystemNameAsync("ConditionsOfUse").Result is not { } topic)
            return;

        if (topic.Body?.Trim() == ConditionsOfUseTopic.ArabicPlaceholder)
        {
            topic.Title = ConditionsOfUseTopic.ArabicTitle;
            topic.Body = ConditionsOfUseTopic.ArabicBody;
            topicService.UpdateTopicAsync(topic).Wait();
        }

        //the seeded body is Arabic; English rides along as a localized value
        var english = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("en", StringComparison.OrdinalIgnoreCase));
        if (english is null)
            return;

        var body = localizationService.GetLocalizedAsync(topic, entity => entity.Body, english.Id, returnDefaultValue: false).Result;
        if (!string.IsNullOrEmpty(body))
            return;

        localizedEntityService.SaveLocalizedValueAsync(topic, entity => entity.Title, ConditionsOfUseTopic.EnglishTitle, english.Id).Wait();
        localizedEntityService.SaveLocalizedValueAsync(topic, entity => entity.Body, ConditionsOfUseTopic.EnglishBody, english.Id).Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}