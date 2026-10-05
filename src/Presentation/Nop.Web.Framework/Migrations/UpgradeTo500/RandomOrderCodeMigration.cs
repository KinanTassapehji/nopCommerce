using FluentMigrator;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Orders;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Order numbers were the sequential Id ("{ID}"), which tells a customer how many orders the
/// store has had. The number customers see (CustomOrderNumber) becomes a random code,
/// "LS-{CODE}"; the Id stays sequential for admins. Orders still numbered by their Id get a
/// code too. A mask an admin already changed, and numbers not equal to the Id, are left alone.
/// </summary>
[NopUpdateMigration("2026-09-29 20:00:00", "5.00", UpdateMigrationType.Data)]
public class RandomOrderCodeMigration : MigrationBase
{
    protected const string MASK = "LS-{CODE}";

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var orderRepository = EngineContext.Current.Resolve<IRepository<Order>>();

        var orderSettings = settingService.LoadSetting<OrderSettings>();
        if (!string.IsNullOrEmpty(orderSettings.CustomOrderNumberMask) && orderSettings.CustomOrderNumberMask != "{ID}")
            return;

        orderSettings.CustomOrderNumberMask = MASK;
        settingService.SaveSetting(orderSettings);

        //the same generator new orders go through, on the mask just saved
        var formatter = new CustomNumberFormatter(orderRepository, orderSettings);
        var orders = orderRepository.Table.ToList()
            .Where(order => order.CustomOrderNumber == order.Id.ToString())
            .ToList();
        foreach (var order in orders)
        {
            order.CustomOrderNumber = formatter.GenerateOrderCustomNumber(order);
            orderRepository.UpdateAsync(order, false).Wait();
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}