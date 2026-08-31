using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Settings;

/// <summary>
/// Represents a shopping cart settings model
/// </summary>
public partial record ShoppingCartSettingsModel : BaseNopModel, ISettingsModel
{
    #region Properties

    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.DisplayCartAfterAddingProduct")]
    public bool DisplayCartAfterAddingProduct { get; set; }
    public bool DisplayCartAfterAddingProduct_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.MaximumShoppingCartItems")]
    public int MaximumShoppingCartItems { get; set; }
    public bool MaximumShoppingCartItems_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.CartsSharedBetweenStores")]
    public bool CartsSharedBetweenStores { get; set; }
    public bool CartsSharedBetweenStores_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowProductImagesOnShoppingCart")]
    public bool ShowProductImagesOnShoppingCart { get; set; }
    public bool ShowProductImagesOnShoppingCart_OverrideForStore { get; set; }


    [NopResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowDiscountBox")]
    public bool ShowDiscountBox { get; set; }
    public bool ShowDiscountBox_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.CrossSellsNumber")]
    public int CrossSellsNumber { get; set; }
    public bool CrossSellsNumber_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.MiniShoppingCartEnabled")]
    public bool MiniShoppingCartEnabled { get; set; }
    public bool MiniShoppingCartEnabled_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.ShowProductImagesInMiniShoppingCart")]
    public bool ShowProductImagesInMiniShoppingCart { get; set; }
    public bool ShowProductImagesInMiniShoppingCart_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.MiniShoppingCartProductNumber")]
    public int MiniShoppingCartProductNumber { get; set; }
    public bool MiniShoppingCartProductNumber_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.AllowCartItemEditing")]
    public bool AllowCartItemEditing { get; set; }
    public bool AllowCartItemEditing_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Settings.ShoppingCart.GroupTierPricesForDistinctShoppingCartItems")]
    public bool GroupTierPricesForDistinctShoppingCartItems { get; set; }
    public bool GroupTierPricesForDistinctShoppingCartItems_OverrideForStore { get; set; }

    #endregion
}