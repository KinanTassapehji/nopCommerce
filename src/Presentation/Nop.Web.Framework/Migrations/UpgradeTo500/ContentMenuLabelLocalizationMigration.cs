using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-25 20:00:02", "5.00", UpdateMigrationType.Localization)]
public class ContentMenuLabelLocalizationMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        //"topics (pages)" is just "pages" to the store staff, "blog comments" had a typo,
        //and "widgets" read as "pieces"
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.ConfigurationSteps.TopicList.Topics1.Title"] = "الصفحات",
                ["Admin.ConfigurationSteps.TopicList.Topics2.Title"] = "الصفحات",
                ["Admin.ContentManagement.Topics"] = "الصفحات",
                ["Admin.Configuration.Settings.Blog.BlockTitle.BlogComments"] = "تعليقات المدونة",
                ["Admin.ContentManagement.Blog.Comments"] = "تعليقات المدونة",
                ["Admin.ContentManagement.Widgets"] = "عناصر الواجهة",
                ["Admin.ContentManagement.Widgets.BackToList"] = "العودة إلى قائمة عناصر الواجهة"
            }, arabic.Id);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}