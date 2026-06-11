using FluentMigrator;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.ScheduleTasks;

namespace Nop.Data.Migrations.UpgradeTo500;

[NopUpdateMigration("2026-03-31 00:00:01", "5.00", UpdateMigrationType.Data)]
public class DataMigration : Migration
{
    private readonly INopDataProvider _dataProvider;

    public DataMigration(INopDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        //#8120
        if (!_dataProvider.GetTable<ScheduleTask>().Any(st => string.Compare(st.Type, "Nop.Services.Orders.AutoCancelOrdersTask, Nop.Services", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(new ScheduleTask()
            {
                Name = "Auto-cancel unpaid orders",
                //60 minutes
                Seconds = 3600,
                Type = "Nop.Services.Orders.AutoCancelOrdersTask, Nop.Services",
                Enabled = true,
                LastEnabledUtc = DateTime.UtcNow,
                StopOnError = false
            });
        }

        var activityLogTypeTable = _dataProvider.GetTable<ActivityLogType>();

        //#1832
        if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, "AddNewContactFormAttribute", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(
                new ActivityLogType
                {
                    SystemKeyword = "AddNewContactFormAttribute",
                    Enabled = true,
                    Name = "Add a new contact form attribute"
                }
            );
        }

        if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, "EditContactFormAttribute", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(
                new ActivityLogType
                {
                    SystemKeyword = "EditContactFormAttribute",
                    Enabled = true,
                    Name = "Edit a contact form attribute"
                }
            );
        }

        if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, "DeleteContactFormAttribute", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(
                new ActivityLogType
                {
                    SystemKeyword = "DeleteContactFormAttribute",
                    Enabled = true,
                    Name = "Delete a contact form attribute"
                }
            );
        }

        if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, "AddNewContactFormAttributeValue", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(
                new ActivityLogType
                {
                    SystemKeyword = "AddNewContactFormAttributeValue",
                    Enabled = true,
                    Name = "Add a new contact form attribute value"
                }
            );
        }

        if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, "EditContactFormAttributeValue", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(
                new ActivityLogType
                {
                    SystemKeyword = "EditContactFormAttributeValue",
                    Enabled = true,
                    Name = "Edit a contact form attribute value"
                }
            );
        }

        if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, "DeleteContactFormAttributeValue", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(
                new ActivityLogType
                {
                    SystemKeyword = "DeleteContactFormAttributeValue",
                    Enabled = true,
                    Name = "Delete a contact form attribute value"
                }
            );
        }

        //#8098
        if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, "AddNewPriceList", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(
                new ActivityLogType
                {
                    SystemKeyword = "AddNewPriceList",
                    Enabled = true,
                    Name = "Add a new price list"
                }
            );
        }

        if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, "DeletePriceList", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(
                new ActivityLogType
                {
                    SystemKeyword = "DeletePriceList",
                    Enabled = true,
                    Name = "Delete a price list"
                }
            );
        }

        if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, "EditPriceList", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(
                new ActivityLogType
                {
                    SystemKeyword = "EditPriceList",
                    Enabled = true,
                    Name = "Edit a price list"
                }
            );
        }

        if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, "ExportPriceLists", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(
                new ActivityLogType
                {
                    SystemKeyword = "ExportPriceLists",
                    Enabled = true,
                    Name = "Export price lists"
                }
            );
        }

        if (!activityLogTypeTable.Any(alt => string.Compare(alt.SystemKeyword, "ImportPriceLists", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            _dataProvider.InsertEntity(
                new ActivityLogType
                {
                    SystemKeyword = "ImportPriceLists",
                    Enabled = true,
                    Name = "Import price lists"
                }
            );
        }

        var comments = from bc in _dataProvider.GetTable<BlogComment>()
                       join c in _dataProvider.GetTable<Customer>() on bc.CustomerId equals c.Id
                       where string.IsNullOrEmpty(c.Email)
                       select bc;

        foreach (var comment in comments.ToList())
        {
            comment.CustomerId = null;
            _dataProvider.UpdateEntity(comment);
        }

        var productReviews = from pr in _dataProvider.GetTable<ProductReview>()
                             join c in _dataProvider.GetTable<Customer>() on pr.CustomerId equals c.Id
                             where string.IsNullOrEmpty(c.Email)
                             select pr;

        foreach (var pr in productReviews.ToList())
        {
            pr.CustomerId = null;
            _dataProvider.UpdateEntity(pr);
        }

        var searchTerms = from term in _dataProvider.GetTable<SearchTerm>()
                          join c in _dataProvider.GetTable<Customer>() on term.CustomerId equals c.Id
                          where string.IsNullOrEmpty(c.Email)
                          select term;

        foreach (var term in searchTerms.ToList())
        {
            term.CustomerId = null;
            _dataProvider.UpdateEntity(term);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}
