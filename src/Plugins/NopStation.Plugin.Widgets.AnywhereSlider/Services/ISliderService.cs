using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Services;

public interface ISliderService
{
	Task<IPagedList<Slider>> GetAllSlidersAsync(List<int> widgetZoneIds = null, int storeId = 0, bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue);

	Task<Slider> GetSliderByIdAsync(int sliderId);

	Task InsertSliderAsync(Slider slider);

	Task UpdateSliderAsync(Slider slider);

	Task DeleteSliderAsync(Slider slider);

	Task<IPagedList<SliderItem>> GetSliderItemsBySliderIdAsync(int sliderId, int pageIndex = 0, int pageSize = int.MaxValue);

	Task<SliderItem> GetSliderItemByIdAsync(int sliderItemId);

	Task InsertSliderItemAsync(SliderItem sliderItem);

	Task UpdateSliderItemAsync(SliderItem sliderItem);

	Task DeleteSliderItemAsync(SliderItem sliderItem);
}
