using FluentMigrator;
using LinqToDB;
using Nop.Core.Domain.Directory;

namespace Nop.Data.Migrations.UpgradeTo500;

/// <summary>
/// Seeds the Syrian governorates.
///
/// The core install ships Syria as a country with no states of its own, so the
/// province dropdown on every address form was empty for the one country this
/// store actually delivers to. The same rows are in
/// App_Data/Localization/states.txt, which is where the installer reads states
/// from - that covers a fresh install, this covers a store already running.
///
/// Names are Arabic only: it is the store's primary language, and a governorate
/// is a proper noun the shopper picks out of a list, not prose to read.
/// </summary>
[NopUpdateMigration("2026-08-29 00:00:02", "5.00", UpdateMigrationType.Data)]
public class DataMigration : ForwardOnlyMigration
{
    #region Fields

    protected readonly INopDataProvider _dataProvider;

    #endregion

    #region Ctor

    public DataMigration(INopDataProvider dataProvider)
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
        var country = _dataProvider.GetTable<Country>()
            .FirstOrDefault(c => c.TwoLetterIsoCode == "SY");

        //a store that has deleted the country wants it gone; do not put it back
        if (country is null)
            return;

        var existing = _dataProvider.GetTable<StateProvince>()
            .Where(sp => sp.CountryId == country.Id)
            .Select(sp => sp.Name)
            .ToList();

        //the 14 governorates, ordered by population so the dropdown opens on the
        //cities most of the orders come from
        var governorates = new[]
        {
            "دمشق",
            "ريف دمشق",
            "حلب",
            "حمص",
            "حماة",
            "اللاذقية",
            "طرطوس",
            "إدلب",
            "دير الزور",
            "الرقة",
            "الحسكة",
            "درعا",
            "السويداء",
            "القنيطرة"
        };

        for (var i = 0; i < governorates.Length; i++)
        {
            //idempotent: a store that already added one by hand keeps its row
            if (existing.Contains(governorates[i], StringComparer.InvariantCultureIgnoreCase))
                continue;

            _dataProvider.InsertEntity(new StateProvince
            {
                CountryId = country.Id,
                Name = governorates[i],
                Published = true,
                DisplayOrder = i
            });
        }
    }

    #endregion
}