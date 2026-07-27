using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Feed.ChatGptShopping.Models;

/// <summary>
/// Represents plugin configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    public ConfigurationModel()
    {
        AvailableCurrencies = new List<SelectListItem>();
        GeneratedFiles = new List<GeneratedFileModel>();
    }

    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Feed.ChatGptShopping.Configuration.Currency")]
    public int CurrencyId { get; set; }
    public IList<SelectListItem> AvailableCurrencies { get; set; }
    public bool CurrencyId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Feed.ChatGptShopping.Configuration.StaticFilePath")]
    public IList<GeneratedFileModel> GeneratedFiles { get; set; }

    [NopResourceDisplayName("Plugins.Feed.ChatGptShopping.Configuration.AutoSyncEnabled")]
    public bool AutoSyncEnabled { get; set; }
    public bool AutoSyncEnabled_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Feed.ChatGptShopping.Configuration.AutoSyncPeriod")]
    public int AutoSyncPeriod { get; set; }
    public bool AutoSyncPeriod_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Feed.ChatGptShopping.Configuration.ProductPictureSize")]
    public int ProductPictureSize { get; set; }
    public bool ProductPictureSize_OverrideForStore { get; set; }

}
