using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-20 00:00:01", "5.00", UpdateMigrationType.Settings)]
public class PdfInvoiceMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

        //The font is picked from the document language's direction, and the LTR font
        //(OpenSans) carries no Arabic glyphs: an invoice generated in English over this
        //catalogue printed every product name as blank. Vazirmatn covers both scripts,
        //so whatever language the invoice is generated in, all of its text is on it.
        var pdfSettings = settingService.LoadSetting<PdfSettings>();
        pdfSettings.LtrFontName = "Vazirmatn";
        settingService.SaveSetting(pdfSettings);

        //...and with that on, the admin's copy of an order placed in English was an
        //English document for an Arabic shop. Both copies of the same invoice now
        //follow the language the store is being read in, which is what the customer
        //facing route already did.
        var orderSettings = settingService.LoadSetting<OrderSettings>();
        orderSettings.GeneratePdfInvoiceInCustomerLanguage = false;
        settingService.SaveSetting(orderSettings);

        //the resource is the format string for the download name, but carried no {0},
        //so every invoice ever downloaded landed on the last one as "order.pdf".
        //One value for every language: a file name is not read, it is sorted and
        //attached, and an ASCII one survives both.
        localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["PDFInvoice.FileName"] = "order-{0}"
        });
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}