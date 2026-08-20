using Nop.Core.Configuration;

namespace Nop.Plugin.Feed.ChatGptShopping;

public class ChatGptShoppingSettings : ISettings
{

    /// <summary>
    /// Currency identifier for which feed file(s) will be generated
    /// </summary>
    public int CurrencyId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether auto synchronization feed is enabled
    /// </summary>
    public bool AutoSyncEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value how often (in minutes) auto synchronization feed will run
    /// </summary>
    public int AutoSyncPeriod { get; set; }

    /// <summary>
    /// Product picture size
    /// </summary>
    public int ProductPictureSize { get; set; }
}
