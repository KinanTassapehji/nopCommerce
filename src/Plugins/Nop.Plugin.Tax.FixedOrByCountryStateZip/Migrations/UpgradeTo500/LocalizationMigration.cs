using FluentMigrator;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Web.Framework.Extensions;

namespace Nop.Plugin.Tax.FixedOrByCountryStateZip.Migrations.UpgradeTo500;

[NopMigration("2026-08-14 12:00:01", "Tax.FixedOrByCountryStateZip 5.0.5. Update localizations", MigrationProcessType.Update)]
public class LocalizationMigration : MigrationBase
{
    public override void Down()
    {
        //add the downgrade logic if necessary 
    }

    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //add, update and delete localization resources
        this.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.Percentage2"] = "Percentage 2",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.Percentage2.Hint"] = "The second tax rate.",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.Percentage3"] = "Percentage 3",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.Percentage3.Hint"] = "The third tax rate.",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.UsePercentage2"] = "Use percentage 2",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.UsePercentage2.Hint"] = "Check this box if you want to use the second tax rate.",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.UsePercentage3"] = "Use percentage 3",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.UsePercentage3.Hint"] = "Check this box if you want to use the third tax rate.",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RenameRate1"] = "Rename rate 1",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RenameRate1.Hint"] = "If chacked, you may change the name of the first tax rate.",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RenameRate2"] = "Rename rate 2",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RenameRate2.Hint"] = "If chacked, you may change the name of the second tax rate.",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RenameRate3"] = "Rename rate 3",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RenameRate3.Hint"] = "If chacked, you may change the name of the third tax rate.",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RateName1"] = "Rate name 1",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RateName1.Hint"] = "The name of the first tax rate.",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RateName2"] = "Rate name 2",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RateName2.Hint"] = "The name of the second tax rate.",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RateName3"] = "Rate name 3",
            ["Plugins.Tax.FixedOrByCountryStateZip.Fields.RateName3.Hint"] = "The name of the third tax rate."
        });
    }
}
