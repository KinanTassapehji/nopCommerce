using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Messages;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-08-23 00:00:02", "5.00", UpdateMigrationType.Data)]
public class MessageTemplateMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var messageTemplateService = EngineContext.Current.Resolve<IMessageTemplateService>();

        //gift cards and wishlists are not used in this store
        string[] obsoleteTemplates =
        [
            "GiftCard.Notification",
            "Wishlist.EmailAFriend"
        ];

        foreach (var name in obsoleteTemplates)
            foreach (var template in messageTemplateService.GetMessageTemplatesByNameAsync(name).Result)
                messageTemplateService.DeleteMessageTemplateAsync(template).Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}