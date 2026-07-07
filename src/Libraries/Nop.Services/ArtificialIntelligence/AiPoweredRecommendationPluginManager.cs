using Nop.Core.Domain.ArtificialIntelligence;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Plugins;

namespace Nop.Services.ArtificialIntelligence;

/// <summary>
/// Represents a search plugin manager implementation
/// </summary>
public partial class AiPoweredRecommendationPluginManager : PluginManager<IAiPoweredRecommendationPlugin>, IAiPoweredRecommendationPluginManager
{
    #region Fields

    protected readonly ArtificialIntelligenceSettings _artificialIntelligenceSettings;

    #endregion

    #region Ctor

    public AiPoweredRecommendationPluginManager(ArtificialIntelligenceSettings artificialIntelligenceSettings, ICustomerService customerService, IPluginService pluginService)
        : base(customerService, pluginService)
    {
        _artificialIntelligenceSettings = artificialIntelligenceSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Load primary active search provider
    /// </summary>
    /// <param name="customer">Filter by customer; pass null to load all plugins</param>
    /// <param name="storeId">Filter by store; pass 0 to load all plugins</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the search provider
    /// </returns>
    public virtual async Task<IAiPoweredRecommendationPlugin> LoadPrimaryPluginAsync(Customer customer = null, int storeId = 0)
    {
        if (string.IsNullOrEmpty(_artificialIntelligenceSettings.ActiveAiPoweredRecommendationProviderSystemName))
            return null;

        return await LoadPrimaryPluginAsync(_artificialIntelligenceSettings.ActiveAiPoweredRecommendationProviderSystemName, customer, storeId);
    }

    /// <summary>
    /// Check whether the passed recommendation provider is active
    /// </summary>
    /// <param name="recommendationProvider">Recommendation provider to check</param>
    /// <returns>Result</returns>
    public virtual bool IsPluginActive(IAiPoweredRecommendationPlugin recommendationProvider)
    {
        return IsPluginActive(recommendationProvider, [_artificialIntelligenceSettings.ActiveAiPoweredRecommendationProviderSystemName]);
    }
    
    /// <summary>
    /// Check whether the AI-powered recommendation provider with the passed system name is active
    /// </summary>
    /// <param name="systemName">System name of recommendation provider to check</param>
    /// <param name="customer">Filter by customer; pass null to load all plugins</param>
    /// <param name="storeId">Filter by store; pass 0 to load all plugins</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public virtual async Task<bool> IsPluginActiveAsync(string systemName, Customer customer = null, int storeId = 0)
    {
        var searchProvider = await LoadPluginBySystemNameAsync(systemName, customer, storeId);
        return IsPluginActive(searchProvider);
    }

    #endregion
}