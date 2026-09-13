using System.Text;
using System.Text.RegularExpressions;
using FluentMigrator;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Topics;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;
using Nop.Services.Topics;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The out-of-warranty price list shipped as bullets - "&lt;strong&gt;name&lt;/strong&gt; - price"
/// thirty times over - which buries the number at the end of a sentence and leaves
/// nothing to scan down. The seed now writes real tables; this rewrites the topic
/// already in the database.
///
/// ponytail: rewrite the body that is there rather than restate 8 KB of copy in a
/// migration, so an admin's own edits to the wording survive. Only a list whose
/// every item is that exact shape is touched; anything else is left alone.
/// </summary>
[NopUpdateMigration("2026-09-12 00:00:04", "5.00", UpdateMigrationType.Data)]
public class MaintenancePriceTableMigration : MigrationBase
{
    //<li><strong>Split Type</strong> - 230 SAR</li>, and nothing else in the list
    protected static readonly Regex _priceList =
        new(@"<ul>(?:<li><strong>[^<>]+</strong>\s*[—-]\s*[^<>]+</li>)+</ul>", RegexOptions.Compiled);

    protected static readonly Regex _priceItem =
        new(@"<li><strong>([^<>]+)</strong>\s*[—-]\s*([^<>]+)</li>", RegexOptions.Compiled);

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();
        var localizedEntityService = EngineContext.Current.Resolve<ILocalizedEntityService>();
        var topicService = EngineContext.Current.Resolve<ITopicService>();

        if (topicService.GetTopicBySystemNameAsync("MaintenanceServices").Result is not { } topic)
            return;

        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));

        //the seeded body is Arabic; English rides along as a localized value
        var tabled = ToTables(topic.Body, "الخدمة", "السعر");
        if (tabled != topic.Body)
        {
            topic.Body = tabled;
            topicService.UpdateTopicAsync(topic).Wait();
        }

        foreach (var language in languageService.GetAllLanguages(showHidden: true).Where(language => language.Id != arabic?.Id))
        {
            var body = localizationService.GetLocalizedAsync(topic, entity => entity.Body, language.Id, returnDefaultValue: false).Result;
            if (string.IsNullOrEmpty(body))
                continue;

            localizedEntityService
                .SaveLocalizedValueAsync(topic, entity => entity.Body, ToTables(body, "Service", "Price"), language.Id)
                .Wait();
        }
    }

    protected static string ToTables(string body, string serviceHeading, string priceHeading)
    {
        if (string.IsNullOrEmpty(body))
            return body;

        return _priceList.Replace(body, list =>
        {
            var table = new StringBuilder()
                .Append("<table><thead><tr><th>").Append(serviceHeading)
                .Append("</th><th>").Append(priceHeading)
                .Append("</th></tr></thead><tbody>");

            foreach (Match item in _priceItem.Matches(list.Value))
                table.Append("<tr><td>").Append(item.Groups[1].Value.Trim())
                    .Append("</td><td>").Append(item.Groups[2].Value.Trim())
                    .Append("</td></tr>");

            return table.Append("</tbody></table>").ToString();
        });
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
