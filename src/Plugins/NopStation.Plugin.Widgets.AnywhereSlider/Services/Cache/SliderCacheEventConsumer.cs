using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Services.Cache;

public class SliderCacheEventConsumer : CacheEventConsumer<Slider>
{
	protected override async Task ClearCacheAsync(Slider entity)
	{
		await RemoveByPrefixAsync(NopEntityCacheDefaults<Slider>.Prefix);
		await RemoveByPrefixAsync(AnywhereSliderCacheDefaults.SliderPrefix);
	}
}
