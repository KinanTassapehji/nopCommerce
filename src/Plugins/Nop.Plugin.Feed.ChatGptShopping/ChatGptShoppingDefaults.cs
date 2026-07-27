namespace Nop.Plugin.Feed.ChatGptShopping;

/// <summary>
/// Represents plugin constants
/// </summary>
public class ChatGptShoppingDefaults
{
    /// <summary>
    /// Gets a plugin system name
    /// </summary>
    public static string SystemName => "Feed.ChatGptShopping";

    /// <summary>
    /// Gets the configuration route name
    /// </summary>
    public static string ConfigurationRouteName => "Plugin.Feed.ChatGptShopping.Configure";

    /// <summary>
    /// Gets a name, type and period (in seconds) of the auto synchronization task
    /// </summary>
    public static (string Name, string Type, int Period) SynchronizationTask =>
        ("Synchronization (ChatGptShopping plugin)", "Nop.Plugin.Feed.ChatGptShopping.Services.ChatGptShoppingSyncTask", 28800);
}
