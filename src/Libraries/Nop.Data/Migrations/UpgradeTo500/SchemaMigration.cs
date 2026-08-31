using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;

namespace Nop.Data.Migrations.UpgradeTo500;

/// <summary>
/// Drops the columns left behind by the features TmTm does not sell into:
/// downloadable products.
///
/// This is not cosmetic. The columns are NOT NULL with no default, so once the
/// entity stops mapping them an INSERT of a new product or order item fails
/// outright — an existing store cannot be left with them in place.
/// </summary>
[NopSchemaMigration("2026-08-29 00:00:01", "SchemaMigration for 5.00.0 - drop downloadable product columns")]
public class SchemaMigration : ForwardOnlyMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        DropColumns(nameof(Product),
            "IsDownload",
            "DownloadId",
            "UnlimitedDownloads",
            "MaxNumberOfDownloads",
            "DownloadExpirationDays",
            "DownloadActivationTypeId",
            "HasSampleDownload",
            "SampleDownloadId",
            "HasUserAgreement",
            "UserAgreementText");

        DropColumns(nameof(OrderItem),
            "DownloadCount",
            "IsDownloadActivated",
            "LicenseDownloadId");
    }

    /// <summary>
    /// Drops each column that is still there, so the migration is safe to run
    /// against a store installed after the feature was removed as well as one
    /// upgraded from before it
    /// </summary>
    /// <param name="tableName">Table name</param>
    /// <param name="columnNames">Column names</param>
    protected virtual void DropColumns(string tableName, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
            if (Schema.Table(tableName).Column(columnName).Exists())
                Delete.Column(columnName).FromTable(tableName);
    }
}
