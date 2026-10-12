using FluentMigrator;
using Nop.Core.Domain.Messages;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Most of the stock message templates are never used here. Super administrators see them all
/// and tick which ones the other administrators see (IMessageTemplateService
/// .GetAdminVisibleMessageTemplateNamesAsync). The first pick is the emails the store actually
/// sends (AdminVisibleTemplates). Set once: a pick a super administrator made survives.
/// </summary>
[NopUpdateMigration("2026-09-29 22:00:00", "5.00", UpdateMigrationType.Data)]
public class MessageTemplateAdminVisibilityMigration : MigrationBase
{
    /// <summary>
    /// The emails this store actually sends: orders, delivery, accounts and contact. The rest
    /// serve features it does not use (blog, forums, news, vendors, VAT, reviews, ...)
    /// </summary>
    protected static readonly string[] AdminVisibleTemplates =
    [
        MessageTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION,
        MessageTemplateSystemNames.ORDER_PLACED_STORE_OWNER_NOTIFICATION,
        MessageTemplateSystemNames.ORDER_CANCELLED_CUSTOMER_NOTIFICATION,
        MessageTemplateSystemNames.ORDER_COMPLETED_CUSTOMER_NOTIFICATION,
        MessageTemplateSystemNames.SHIPMENT_SENT_CUSTOMER_NOTIFICATION,
        MessageTemplateSystemNames.SHIPMENT_DELIVERED_CUSTOMER_NOTIFICATION,
        MessageTemplateSystemNames.CUSTOMER_WELCOME_MESSAGE,
        MessageTemplateSystemNames.CUSTOMER_PASSWORD_RECOVERY_MESSAGE,
        MessageTemplateSystemNames.CUSTOMER_REGISTERED_STORE_OWNER_NOTIFICATION,
        MessageTemplateSystemNames.CONTACT_US_MESSAGE
    ];

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var languageService = EngineContext.Current.Resolve<ILanguageService>();
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var settingService = EngineContext.Current.Resolve<ISettingService>();

        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Admin.ContentManagement.MessageTemplates.Fields.VisibleToAdministrators"] = "Visible to administrators",
            ["Admin.ContentManagement.MessageTemplates.Fields.VisibleToAdministrators.Hint"] = "Administrators who are not super administrators see and edit only the message templates ticked here. Super administrators see them all."
        });

        //ponytail: match on the language prefix - the pack ships as ar-SY, older installs carry ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase));
        if (arabic is not null)
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Admin.ContentManagement.MessageTemplates.Fields.VisibleToAdministrators"] = "ظاهر للمشرفين",
                ["Admin.ContentManagement.MessageTemplates.Fields.VisibleToAdministrators.Hint"] = "المشرفون يرون ويعدّلون فقط قوالب الرسائل المحدد عليها هنا. المشرف العام يرى جميع القوالب."
            }, arabic.Id);

        if (settingService.GetSettingAsync(NopMessageDefaults.AdminVisibleMessageTemplatesSettingKey).Result is not null)
            return;

        settingService.SetSettingAsync(NopMessageDefaults.AdminVisibleMessageTemplatesSettingKey, string.Join(",", AdminVisibleTemplates)).Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}