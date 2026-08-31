using FluentMigrator;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Menus;
using Nop.Core.Domain.Topics;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;
//Nop.Web.Framework.Menu is a namespace in this assembly, so the entity needs an alias here
using MenuEntity = Nop.Core.Domain.Menus.Menu;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Arabic names for the menus, menu items and topics the installer seeds. They are entity data,
/// so the bundled ar-SA language pack cannot carry them - without this the footer and the main
/// menu stay English while the rest of the store is translated.
/// </summary>
/// <remarks>
/// ponytail: a plain NopMigration, not a NopUpdateMigration - update migrations are only marked as
/// applied on a fresh install, and a fresh install seeds exactly the rows this translates.
/// </remarks>
[NopMigration("2026-08-24 00:00:00", "5.00", UpdateMigrationType.Data)]
public class MenuLocalizationMigration : MigrationBase
{
    #region Fields

    protected static readonly Dictionary<string, string> _menuNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Categories"] = "الفئات",
        ["Menu"] = "القائمة",
        ["Information"] = "معلومات",
        ["Customer service"] = "خدمة العملاء",
        ["My account"] = "حسابي"
    };

    protected static readonly Dictionary<string, string> _menuItemTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Home page"] = "الصفحة الرئيسية",
        ["New products"] = "منتجات جديدة",
        ["Search"] = "بحث",
        ["My account"] = "حسابي",
        ["Blog"] = "مدونة",
        ["Contact us"] = "اتصل بنا",
        ["Sitemap"] = "خريطة الموقع",
        ["News"] = "الاخبار",
        ["Recently viewed products"] = "منتجات شوهدت مؤخرا",
        ["Compare products list"] = "قائمة مقارنة المنتجات",
        ["Orders"] = "الطلبات",
        ["Addresses"] = "العناوين",
        ["Shopping cart"] = "سلة التسوق",
        ["Apply for vendor account"] = "تسجيل كبائع معنا"
    };

    //topics are matched by system name, their titles are store content and may already be edited
    protected static readonly Dictionary<string, string> _topicTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AboutUs"] = "من نحن",
        ["ConditionsOfUse"] = "شروط الاستخدام",
        ["ContactUs"] = "اتصل بنا",
        ["PrivacyInfo"] = "إشعار الخصوصية",
        ["ShippingInfo"] = "الشحن والإرجاع"
    };

    #endregion

    #region Methods

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var arabic = EngineContext.Current.Resolve<ILanguageService>().GetAllLanguages(showHidden: true)
            .FirstOrDefault(language => language.LanguageCulture == "ar-SA");

        if (arabic is null)
            return;

        var localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>();

        //anything already translated (by hand in the admin area, or by an earlier run) wins
        var translated = localizedPropertyRepository.Table
            .Where(property => property.LanguageId == arabic.Id)
            .Select(property => new { property.LocaleKeyGroup, property.LocaleKey, property.EntityId })
            .ToList()
            .Select(property => $"{property.LocaleKeyGroup}.{property.LocaleKey}.{property.EntityId}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var properties = new List<LocalizedProperty>();

        void translate(string localeKeyGroup, string localeKey, int entityId, string localeValue)
        {
            if (!translated.Add($"{localeKeyGroup}.{localeKey}.{entityId}"))
                return;

            properties.Add(new LocalizedProperty
            {
                EntityId = entityId,
                LanguageId = arabic.Id,
                LocaleKeyGroup = localeKeyGroup,
                LocaleKey = localeKey,
                LocaleValue = localeValue
            });
        }

        foreach (var menu in EngineContext.Current.Resolve<IRepository<MenuEntity>>().Table.ToList())
            if (_menuNames.TryGetValue(menu.Name ?? string.Empty, out var name))
                translate("Menu", nameof(MenuEntity.Name), menu.Id, name);

        foreach (var menuItem in EngineContext.Current.Resolve<IRepository<MenuItem>>().Table.ToList())
            if (_menuItemTitles.TryGetValue(menuItem.Title ?? string.Empty, out var title))
                translate(nameof(MenuItem), nameof(MenuItem.Title), menuItem.Id, title);

        foreach (var topic in EngineContext.Current.Resolve<IRepository<Topic>>().Table.ToList())
            if (_topicTitles.TryGetValue(topic.SystemName ?? string.Empty, out var title))
                translate(nameof(Topic), nameof(Topic.Title), topic.Id, title);

        if (properties.Any())
            localizedPropertyRepository.Insert(properties, false);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }

    #endregion
}