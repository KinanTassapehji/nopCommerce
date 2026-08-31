using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Infrastructure.Cache;

public class SliderCacheDefaults
{
	public static CacheKey SliderBackgrounPictureKey => new CacheKey("Nopstation.anywhereslider.slidermodel.backgrounpicture.{0}-{1}");

	public static CacheKey SliderModelKey => new CacheKey("Nopstation.anywhereslider.slidermodel.byid.{0}-{1}-{2}-{3}-{4}");

	public static string SliderModelPrefix => "Nopstation.anywhereslider.slidermodel.";
}
