using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Common;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;
using NopStation.Plugin.Widgets.AnywhereSlider.Services.Cache;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Services;

public class SliderService : ISliderService
{
	private readonly IStaticCacheManager _cacheManager;

	private readonly IRepository<SliderItem> _sliderItemRepository;

	private readonly IRepository<Slider> _sliderRepository;

	private readonly IStoreMappingService _storeMappingService;

	public SliderService(IRepository<Slider> sliderRepository, IRepository<SliderItem> sliderItemRepository, IStaticCacheManager cacheManager, IStoreMappingService storeMappingService)
	{
		_sliderRepository = sliderRepository;
		_sliderItemRepository = sliderItemRepository;
		_storeMappingService = storeMappingService;
		_cacheManager = cacheManager;
	}

	public virtual async Task<IPagedList<Slider>> GetAllSlidersAsync(List<int> widgetZoneIds = null, int storeId = 0, bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		CacheKey key = _cacheManager.PrepareKeyForDefaultCache(AnywhereSliderCacheDefaults.SlidersAllKey, widgetZoneIds, storeId, active);
		return new PagedList<Slider>(await _cacheManager.GetAsync(key, async delegate
		{
			IQueryable<Slider> queryable = _sliderRepository.Table.Where((Slider s) => !s.Deleted && (!((bool?)active).HasValue || s.Active == ((bool?)active).Value));
			if (!widgetZoneIds.IsNullOrEmpty())
			{
				queryable = queryable.Where((Slider s) => widgetZoneIds.Contains(s.WidgetZoneId));
			}
			queryable = (await _storeMappingService.ApplyStoreMapping(queryable, storeId)).OrderBy((Slider s) => s.DisplayOrder);
			return await queryable.ToListAsync();
		}), pageIndex, pageSize);
	}

	public virtual async Task<Slider> GetSliderByIdAsync(int sliderId)
	{
		if (sliderId == 0)
		{
			return null;
		}
		return await _sliderRepository.GetByIdAsync(sliderId, (ICacheKeyService cache) => _cacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<Slider>.ByIdCacheKey, sliderId));
	}

	public virtual async Task InsertSliderAsync(Slider slider)
	{
		await _sliderRepository.InsertAsync(slider);
	}

	public virtual async Task UpdateSliderAsync(Slider slider)
	{
		await _sliderRepository.UpdateAsync(slider);
	}

	public virtual async Task DeleteSliderAsync(Slider slider)
	{
		await _sliderRepository.DeleteAsync(slider);
	}

	public virtual async Task<IPagedList<SliderItem>> GetSliderItemsBySliderIdAsync(int sliderId, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		CacheKey key = _cacheManager.PrepareKeyForDefaultCache(AnywhereSliderCacheDefaults.SliderItemsBySliderIdKey, sliderId);
		return new PagedList<SliderItem>(await _cacheManager.GetAsync(key, async () => await (from si in _sliderItemRepository.Table
			where si.SliderId == sliderId
			orderby si.DisplayOrder
			select si).ToListAsync()), pageIndex, pageSize);
	}

	public virtual async Task<SliderItem> GetSliderItemByIdAsync(int sliderItemId)
	{
		if (sliderItemId == 0)
		{
			return null;
		}
		return await _sliderItemRepository.GetByIdAsync(sliderItemId, (ICacheKeyService cache) => _cacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<SliderItem>.ByIdCacheKey, sliderItemId));
	}

	public async Task InsertSliderItemAsync(SliderItem sliderItem)
	{
		await _sliderItemRepository.InsertAsync(sliderItem);
	}

	public virtual async Task UpdateSliderItemAsync(SliderItem sliderItem)
	{
		await _sliderItemRepository.UpdateAsync(sliderItem);
	}

	public virtual async Task DeleteSliderItemAsync(SliderItem sliderItem)
	{
		await _sliderItemRepository.DeleteAsync(sliderItem);
	}
}
