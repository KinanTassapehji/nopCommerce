using Nop.Core.Caching;

namespace Nop.Plugin.Misc.PunchOut;

/// <summary>
/// Represents plugin constants
/// </summary>
public class PunchOutDefaults
{
    /// <summary>
    /// Gets a plugin system name
    /// </summary>
    public static string SystemName => "Misc.PunchOut";

    /// <summary>
    /// Gets the token length for PunchOut session token generation
    /// </summary>
    public static int TokenLength => 32;

    /// <summary>
    /// Gets the time in hours after which the PunchOut session will be expired
    /// </summary>
    public static int TimeToExpireSession => 24;

    /// <summary>
    /// Gets the configuration route name
    /// </summary>
    public static string ConfigurationRouteName => "Plugin.Misc.PunchOut.Configure";

    /// <summary>
    /// Gets the generic attribute name to hide search block on the plugin configuration page
    /// </summary>
    public static string HideSearchLogBlockAttribute => "PunchOut.HideSearchLogBlock";

    /// <summary>
    /// Gets the generic attribute name to hide search block on the plugin configuration page
    /// </summary>
    public static string HideSearchIdentityBlockAttribute => "PunchOut.HideSearchIdentityBlock";

    /// <summary>
    /// Gets the generic attribute name to hide general settings block on the plugin configuration page
    /// </summary>
    public static string HideGeneralBlock => "PunchOut.HideGeneralBlock";

    /// <summary>
    /// Gets the generic attribute name to hide identity block on the plugin configuration page
    /// </summary>
    public static string HideIdentityBlock => "PunchOut.HideIdentityBlock";

    /// <summary>
    /// Gets the generic attribute name to hide log block on the plugin configuration page
    /// </summary>
    public static string HideLogBlock => "PunchOut.HideLogBlock";

    #region Cache keys

    /// <summary>
    /// Gets a key to cache PunchOut session token
    /// </summary>
    /// <remarks>
    /// {0} : session token
    /// </remarks>
    public static CacheKey SessionTokenCacheKey => new("PunchOut.session.token.{0}");

    #endregion

    #region Generic attributes for PunchOut session

    /// <summary>
    /// Customer attribute key for storing session token
    /// </summary>
    public static string PunchOutSessionTokenAttribute => "PunchOutSessionToken";

    #endregion
}
