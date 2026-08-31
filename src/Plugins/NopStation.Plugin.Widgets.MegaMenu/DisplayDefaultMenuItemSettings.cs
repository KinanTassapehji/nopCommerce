using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.MegaMenu;

/// <summary>
/// Which of the built-in top menu items the mega menu renders.
/// </summary>
/// <remarks>
/// ponytail: nopCommerce 4.90 dropped Nop.Core.Domain.Catalog.DisplayDefaultMenuItemSettings in favour of the
/// DB-driven Nop.Core.Domain.Menus feature. This keeps the plugin's original toggles (auto-registered like any
/// other ISettings, defaults = show everything). Wire it to the new Menus tables if the store starts managing
/// its top menu from the admin menu editor.
/// </remarks>
public partial class DisplayDefaultMenuItemSettings : ISettings
{
    public bool DisplayHomepageMenuItem { get; set; } = true;

    public bool DisplayNewProductsMenuItem { get; set; } = true;

    public bool DisplayProductSearchMenuItem { get; set; } = true;

    public bool DisplayCustomerInfoMenuItem { get; set; } = true;

    public bool DisplayBlogMenuItem { get; set; } = true;

    public bool DisplayForumsMenuItem { get; set; } = true;

    public bool DisplayContactUsMenuItem { get; set; } = true;
}
