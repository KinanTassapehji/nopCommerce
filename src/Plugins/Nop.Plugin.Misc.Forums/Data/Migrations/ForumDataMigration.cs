using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Forums.Domain;

namespace Nop.Plugin.Misc.Forums.Data.Migrations;

[NopMigration("2024-05-29 00:00:00", "Misc.Forums 5.00. Data", MigrationProcessType.Update)]
public class ForumDataMigration : MigrationBase
{
    #region Field

    private readonly INopDataProvider _dataProvider;

    #endregion

    #region Ctor
    public ForumDataMigration(INopDataProvider dataProvider)
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
        var topics = from ft in _dataProvider.GetTable<ForumTopic>()
                       join c in _dataProvider.GetTable<Customer>() on ft.CustomerId equals c.Id
                       where string.IsNullOrEmpty(c.Email)
                       select ft;

        foreach (var topic in topics.ToList())
        {
            topic.CustomerId = null;
            _dataProvider.UpdateEntity(topic);
        }

        var posts = from fp in _dataProvider.GetTable<ForumPost>()
                    join c in _dataProvider.GetTable<Customer>() on fp.CustomerId equals c.Id
                    where string.IsNullOrEmpty(c.Email)
                    select fp;

        foreach (var post in posts.ToList())
        {
            post.CustomerId = null;
            _dataProvider.UpdateEntity(post);
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
