using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Web.Models.Customer;

/// <summary>
/// Cities offered by the "City" dropdown on the registration and account info forms.
/// The stored value is always the English name, so the data does not depend on the
/// language the customer happened to be browsing in.
/// </summary>
public static partial class StoreCities
{
    //ponytail: flat hardcoded list - move to StateProvince records only if the store
    //ever needs per-city shipping/tax rules, which it does not today.
    private static readonly (string English, string Arabic)[] _cities =
    [
        ("Riyadh", "الرياض"),
        ("Jeddah", "جدة"),
        ("Makkah", "مكة المكرمة"),
        ("Madinah", "المدينة المنورة"),
        ("Dammam", "الدمام"),
        ("Khobar", "الخبر"),
        ("Dhahran", "الظهران"),
        ("Al-Ahsa", "الأحساء"),
        ("Jubail", "الجبيل"),
        ("Qatif", "القطيف"),
        ("Taif", "الطائف"),
        ("Tabuk", "تبوك"),
        ("Buraidah", "بريدة"),
        ("Unaizah", "عنيزة"),
        ("Hail", "حائل"),
        ("Khamis Mushait", "خميس مشيط"),
        ("Abha", "أبها"),
        ("Najran", "نجران"),
        ("Jazan", "جازان"),
        ("Yanbu", "ينبع"),
        ("Al-Baha", "الباحة"),
        ("Arar", "عرعر"),
        ("Sakaka", "سكاكا")
    ];

    /// <summary>
    /// Gets the city list for the current working language
    /// </summary>
    public static List<SelectListItem> Items()
    {
        var arabic = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";

        return _cities
            .Select(city => new SelectListItem { Value = city.English, Text = arabic ? city.Arabic : city.English })
            .ToList();
    }
}
