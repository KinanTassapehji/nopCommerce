using FluentMigrator;
using Nop.Core.Caching;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The store trades in Syrian pounds, with US dollars alongside; the rest of the
/// installer's sample currencies go. The primary store / exchange rate currencies are never deleted.
/// </summary>
[NopUpdateMigration("2026-09-26 16:00:00", "5.00", UpdateMigrationType.Data)]
public class CurrencyCleanupMigration : MigrationBase
{
    private static readonly string[] _keep = ["SYP", "USD"];

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var dataProvider = EngineContext.Current.Resolve<INopDataProvider>();
        var currencySettings = EngineContext.Current.Resolve<CurrencySettings>();

        var doomed = dataProvider.GetTable<Currency>().ToList()
            .Where(currency => !_keep.Contains(currency.CurrencyCode, StringComparer.OrdinalIgnoreCase)
                && currency.Id != currencySettings.PrimaryStoreCurrencyId
                && currency.Id != currencySettings.PrimaryExchangeRateCurrencyId)
            .ToList();

        foreach (var currency in doomed)
        {
            foreach (var mapping in dataProvider.GetTable<StoreMapping>().Where(mapping => mapping.EntityName == nameof(Currency) && mapping.EntityId == currency.Id).ToList())
                dataProvider.DeleteEntity(mapping);
            foreach (var property in dataProvider.GetTable<LocalizedProperty>().Where(property => property.LocaleKeyGroup == nameof(Currency) && property.EntityId == currency.Id).ToList())
                dataProvider.DeleteEntity(property);
            foreach (var language in dataProvider.GetTable<Language>().Where(language => language.DefaultCurrencyId == currency.Id).ToList())
            {
                language.DefaultCurrencyId = 0;
                dataProvider.UpdateEntity(language);
            }

            dataProvider.DeleteEntity(currency);
        }

        //currencies were deleted past the services, so drop whatever start-up already cached
        EngineContext.Current.Resolve<IStaticCacheManager>().ClearAsync().Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}