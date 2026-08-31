using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Services.Cache;

public class SliderItemCacheEventConsumer : CacheEventConsumer<SliderItem>
{
	protected override async Task ClearCacheAsync(SliderItem entity)
	{
		await RemoveByPrefixAsync(NopEntityCacheDefaults<SliderItem>.Prefix);
		await RemoveByPrefixAsync(AnywhereSliderCacheDefaults.SliderItemPrefix);
		await RemoveByPrefixAsync(AnywhereSliderCacheDefaults.SliderPrefix);
	}
}
