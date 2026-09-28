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
/// The store trades in Saudi riyals, with US dollars alongside; the rest of the
/// installer's sample currencies go. The primary store / exchange rate currencies are never deleted.
/// The installer never seeded SAR, so it is added when missing (the primary currency is left alone).
/// </summary>
[NopUpdateMigration("2026-09-26 16:00:00", "5.00", UpdateMigrationType.Data)]
public class CurrencyCleanupMigration : MigrationBase
{
    private static readonly string[] _keep = ["SAR", "USD"];

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var dataProvider = EngineContext.Current.Resolve<INopDataProvider>();
        var currencySettings = EngineContext.Current.Resolve<CurrencySettings>();
        var currencies = dataProvider.GetTable<Currency>().ToList();

        if (!currencies.Any(currency => currency.CurrencyCode.Equals("SAR", StringComparison.OrdinalIgnoreCase)))
        {
            //the riyal is pegged at 3.75 to the dollar; the rate is relative to the primary exchange rate currency
            var usd = currencies.FirstOrDefault(currency => currency.CurrencyCode.Equals("USD", StringComparison.OrdinalIgnoreCase));
            dataProvider.InsertEntity(new Currency
            {
                Name = "Saudi Riyal",
                CurrencyCode = "SAR",
                Rate = 3.75M * (usd?.Rate ?? 1M),
                DisplayLocale = "ar-SA",
                CustomFormatting = string.Empty,
                Published = true,
                DisplayOrder = 0,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                RoundingType = RoundingType.Rounding001
            });
        }

        var doomed = currencies
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

        //currencies were written past the services, so drop whatever start-up already cached
        EngineContext.Current.Resolve<IStaticCacheManager>().ClearAsync().Wait();
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}