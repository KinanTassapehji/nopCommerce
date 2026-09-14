using FluentMigrator;
using Nop.Core.Domain.News;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Topics;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-14 00:00:00", "5.00", UpdateMigrationType.Data)]
public class HomepageNewsAndContactTitleMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var topicService = EngineContext.Current.Resolve<ITopicService>();

        //no news block on the home page. The component is already gated on this
        //setting, so nothing in Index.cshtml has to change.
        var newsSettings = settingService.LoadSetting<NewsSettings>();
        newsSettings.ShowNewsOnMainPage = false;
        settingService.SaveSetting(newsSettings);

        //ContactUs.cshtml prints its own <h1>, then renders the ContactUs topic,
        //whose title landed in the database as the same words - so the page read
        //"اتصل بنا" twice. TopicBlock already skips an empty title; the seed
        //ships one, this clears the stores installed before it did.
        var contactUs = topicService.GetTopicBySystemNameAsync("ContactUs").Result;
        if (contactUs is null)
            return;

        if (!string.IsNullOrEmpty(contactUs.Title))
        {
            contactUs.Title = string.Empty;
            topicService.UpdateTopicAsync(contactUs).Wait();
        }

        //the base column is only half of it - the title the page actually prints
        //is the per-language one, so every language has to be cleared as well.
        //An empty value deletes the LocalizedProperty row.
        var localizedEntityService = EngineContext.Current.Resolve<ILocalizedEntityService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        foreach (var language in languageService.GetAllLanguages(showHidden: true))
            localizedEntityService.SaveLocalizedValueAsync(contactUs, topic => topic.Title, string.Empty, language.Id).Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
