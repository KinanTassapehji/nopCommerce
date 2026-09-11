using FluentMigrator;
using LinqToDB;
using Nop.Core.Domain.Directory;

namespace Nop.Data.Migrations.UpgradeTo500;

/// <summary>
/// Seeds the Saudi regions.
///
/// Stock nopCommerce ships 13 English region rows for SA plus "Eastern Cape",
/// which belongs to South Africa - so the province dropdown on every address
/// form was wrong for the one country this store delivers to. The same rows are
/// in App_Data/Localization/states.txt, which is where the installer reads
/// states from - that covers a fresh install, this covers a store already
/// running.
///
/// Existing English rows are renamed in place rather than replaced: an Address
/// may already point at one, and deleting the row would orphan the FK.
///
/// Names are Arabic only: it is the store's primary language, and a region is a
/// proper noun the shopper picks out of a list, not prose to read.
/// </summary>
[NopUpdateMigration("2026-09-10 00:00:01", "5.00", UpdateMigrationType.Data)]
public class SaudiRegionsDataMigration : ForwardOnlyMigration
{
    #region Fields

    protected readonly INopDataProvider _dataProvider;

    #endregion

    #region Ctor

    public SaudiRegionsDataMigration(INopDataProvider dataProvider)
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
            .FirstOrDefault(c => c.TwoLetterIsoCode == "SA");

        //a store that has deleted the country wants it gone; do not put it back
        if (country is null)
            return;

        var existing = _dataProvider.GetTable<StateProvince>()
            .Where(sp => sp.CountryId == country.Id)
            .ToList();

        //the 13 regions, ordered by population so the dropdown opens on the
        //cities most of the orders come from, paired with the stock English name
        //each one replaces
        var regions = new[]
        {
            ("Al Riyadh", "الرياض"),
            ("Makkah", "مكة المكرمة"),
            ("Eastern Province", "المنطقة الشرقية"),
            ("Asir", "عسير"),
            ("Al Madinah", "المدينة المنورة"),
            ("Al Qasim", "القصيم"),
            ("Jizan", "جازان"),
            ("Tabuk", "تبوك"),
            ("Ha'il", "حائل"),
            ("Najran", "نجران"),
            ("Al Jawf", "الجوف"),
            ("Al Bahah", "الباحة"),
            ("Northern Borders", "الحدود الشمالية")
        };

        for (var i = 0; i < regions.Length; i++)
        {
            var (english, arabic) = regions[i];

            //idempotent: already Arabic, only the ordering may have moved
            var row = existing.FirstOrDefault(sp => string.Equals(sp.Name, arabic, StringComparison.InvariantCultureIgnoreCase))
                ?? existing.FirstOrDefault(sp => string.Equals(sp.Name, english, StringComparison.InvariantCultureIgnoreCase));

            if (row is not null)
            {
                //rename in place - an Address may reference this row
                row.Name = arabic;
                row.Published = true;
                row.DisplayOrder = i;
                _dataProvider.UpdateEntity(row);
                continue;
            }

            _dataProvider.InsertEntity(new StateProvince
            {
                CountryId = country.Id,
                Name = arabic,
                Published = true,
                DisplayOrder = i
            });
        }

        //"Eastern Cape" is a South African province that stock nopCommerce files
        //under SA. Unpublish rather than delete: an Address may point at it.
        var misfiled = existing.FirstOrDefault(sp => string.Equals(sp.Name, "Eastern Cape", StringComparison.InvariantCultureIgnoreCase));
        if (misfiled is not null && misfiled.Published)
        {
            misfiled.Published = false;
            _dataProvider.UpdateEntity(misfiled);
        }
    }

    #endregion
}