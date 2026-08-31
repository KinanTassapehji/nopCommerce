using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Factories;

public interface IMarketplaceModelFactory
{
	Task<MarketplaceModel> PrepareMarketplaceListModelAsync(MarketplaceSearchModel searchModel);
}
