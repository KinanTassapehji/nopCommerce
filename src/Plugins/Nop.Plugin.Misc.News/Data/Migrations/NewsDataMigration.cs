using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.News.Domain;

namespace Nop.Plugin.Misc.News.Data.Migrations;

[NopMigration("2026-07-07 00:00:00", "Misc.News 5.00. Data", MigrationProcessType.Update)]
public class NewsDataMigration : MigrationBase
{
    #region Field

    private readonly INopDataProvider _dataProvider;

    #endregion

    #region Ctor

    public NewsDataMigration(INopDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        var comments = from comment in _dataProvider.GetTable<NewsComment>()
                     join c in _dataProvider.GetTable<Customer>() on comment.CustomerId equals c.Id
                     where string.IsNullOrEmpty(c.Email)
                     select comment;

        foreach (var comment in comments.ToList())
        {
            comment.CustomerId = null;
            _dataProvider.UpdateEntity(comment);
        }
    }

    /// <summary>
    /// Collects the DOWN migration expressions
    /// </summary>
    public override void Down()
    {
        //nothing
    }

    #endregion
}
