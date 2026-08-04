using FluentMigrator;
using Nop.Core.Domain;
using Nop.Core.Domain.ArtificialIntelligence;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.PriceLists;
using Nop.Core.Domain.Reminders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Web.Framework.Extensions;

namespace Nop.Plugin.Tax.FixedOrByCountryStateZip.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-08-14 12:00:02", "5.00", UpdateMigrationType.Settings)]
public class SettingMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        this.SetSettingIfNotExists<FixedOrByCountryStateZipTaxSettings, bool>(settings => settings.UsePercentage2, false);
        this.SetSettingIfNotExists<FixedOrByCountryStateZipTaxSettings, bool>(settings => settings.UsePercentage3, false);
        this.SetSettingIfNotExists<FixedOrByCountryStateZipTaxSettings, bool>(settings => settings.RenameRate1, false);
        this.SetSettingIfNotExists<FixedOrByCountryStateZipTaxSettings, bool>(settings => settings.RenameRate2, false);
        this.SetSettingIfNotExists<FixedOrByCountryStateZipTaxSettings, bool>(settings => settings.RenameRate3, false);
        this.SetSettingIfNotExists<FixedOrByCountryStateZipTaxSettings, string>(settings => settings.RateName1, "GST");
        this.SetSettingIfNotExists<FixedOrByCountryStateZipTaxSettings, string>(settings => settings.RateName2, "PST");
        this.SetSettingIfNotExists<FixedOrByCountryStateZipTaxSettings, string>(settings => settings.RateName3, "HST");
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}
