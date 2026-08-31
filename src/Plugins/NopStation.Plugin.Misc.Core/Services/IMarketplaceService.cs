using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Domains.Marketplace;

namespace NopStation.Plugin.Misc.Core.Services;

public interface IMarketplaceService
{
	Task<ApiResponse> GetMarketplaceProductsAsync(int categoryId = 0, int paidFilter = 0, string searchText = null, int versionFilter = 0, int page = 1);

	Task<IList<string>> DownloadAndInstallPluginAsync(int productId, string systemName);

	Task SendUpgradeRequestAsync(string systemName, string version);
}
