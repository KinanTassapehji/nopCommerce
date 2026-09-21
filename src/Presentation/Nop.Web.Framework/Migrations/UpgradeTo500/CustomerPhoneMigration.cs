using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-09-20 00:00:02", "5.00", UpdateMigrationType.Settings)]
public class CustomerPhoneMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();

        //The account page and the registration form already render a phone field -
        //they just never showed it, because this was off. A store that delivers by
        //courier and takes cash on delivery needs a number to call.
        //Not required: the field arrives on accounts that already exist, and a
        //required field they never filled would block them from saving anything else.
        var customerSettings = settingService.LoadSetting<CustomerSettings>();
        customerSettings.PhoneEnabled = true;
        settingService.SaveSetting(customerSettings);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}