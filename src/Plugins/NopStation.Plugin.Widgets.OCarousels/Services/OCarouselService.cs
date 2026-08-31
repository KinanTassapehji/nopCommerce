using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Data;
using NopStation.Plugin.Widgets.OCarousels.Domains;

namespace NopStation.Plugin.Widgets.OCarousels.Services;

public class OCarouselService : IOCarouselService
{
	private const string OCAROUSEL_PATTERN_KEY = "NS.OCarouselList.";

	private readonly IStaticCacheManager _cacheManager;

	private readonly IRepository<OCarousel> _carouselRepository;

	private readonly IRepository<OCarouselItem> _carouselItemRepository;

	private readonly IRepository<StoreMapping> _storeMappingRepository;

	private readonly CatalogSettings _catalogSettings;

	public OCarouselService(IStaticCacheManager cacheManager, IRepository<OCarousel> carouselRepository, IRepository<OCarouselItem> carouselItemRepository, IRepository<StoreMapping> storeMappingRepository, CatalogSettings catalogSettings)
	{
		_cacheManager = cacheManager;
		_carouselRepository = carouselRepository;
		_carouselItemRepository = carouselItemRepository;
		_storeMappingRepository = storeMappingRepository;
		_catalogSettings = catalogSettings;
	}

	public virtual async Task<IPagedList<OCarousel>> GetAllCarouselsAsync(List<int> widgetZoneIds = null, List<int> dataSources = null, int storeId = 0, bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		IQueryable<OCarousel> queryable = _carouselRepository.Table.Where((OCarousel x) => !x.Deleted);
		if (widgetZoneIds != null && widgetZoneIds.Any())
		{
			queryable = queryable.Where((OCarousel carousel) => widgetZoneIds.Contains(carousel.WidgetZoneId));
		}
		if (dataSources != null && dataSources.Any())
		{
			queryable = queryable.Where((OCarousel carousel) => dataSources.Contains(carousel.DataSourceTypeId));
		}
		if (active.HasValue)
		{
			queryable = queryable.Where((OCarousel carousel) => carousel.Active == ((bool?)active).Value);
		}
		if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
		{
			queryable = from o in queryable
				join sm in _storeMappingRepository.Table on new
				{
					c1 = o.Id,
					c2 = "OCarousel"
				} equals new
				{
					c1 = sm.EntityId,
					c2 = sm.EntityName
				} into carousel_sm
				from sm in carousel_sm.DefaultIfEmpty()
				where !o.LimitedToStores || storeId == sm.StoreId
				select o;
		}
		queryable = queryable.OrderBy((OCarousel carousel) => carousel.DisplayOrder);
		return await queryable.ToPagedListAsync(pageIndex, pageSize);
	}

	public virtual async Task<OCarousel> GetCarouselByIdAsync(int carouselId)
	{
		if (carouselId == 0)
		{
			return null;
		}
		return await _carouselRepository.GetByIdAsync(carouselId, (ICacheKeyService cache) => (CacheKey)null);
	}

	public virtual async Task InsertCarouselAsync(OCarousel oCarousel)
	{
		await _carouselRepository.InsertAsync(oCarousel);
		await _cacheManager.RemoveByPrefixAsync("NS.OCarouselList.");
	}

	public virtual async Task UpdateCarouselAsync(OCarousel oCarousel)
	{
		await _carouselRepository.UpdateAsync(oCarousel);
		await _cacheManager.RemoveByPrefixAsync("NS.OCarouselList.");
	}

	public virtual async Task DeleteCarouselAsync(OCarousel oCarousel)
	{
		oCarousel.Deleted = true;
		await _carouselRepository.UpdateAsync(oCarousel);
		await _cacheManager.RemoveByPrefixAsync("NS.OCarouselList.");
	}

	public virtual async Task<IPagedList<OCarouselItem>> GetOCarouselItemsByOCarouselIdAsync(int carouselId, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		return await (from carouselItem in _carouselItemRepository.Table
			where carouselItem.OCarouselId == carouselId
			orderby carouselItem.DisplayOrder
			select carouselItem).ToPagedListAsync(pageIndex, pageSize);
	}

	public virtual async Task<OCarouselItem> GetOCarouselItemByIdAsync(int carouselItemId)
	{
		if (carouselItemId == 0)
		{
			return null;
		}
		return await _carouselItemRepository.GetByIdAsync(carouselItemId, (ICacheKeyService cache) => (CacheKey)null);
	}

	public async Task InsertOCarouselItemAsync(OCarouselItem carouselItem)
	{
		await _carouselItemRepository.InsertAsync(carouselItem);
		await _cacheManager.RemoveByPrefixAsync("NS.OCarouselList.");
	}

	public virtual async Task UpdateOCarouselItemAsync(OCarouselItem carouselItem)
	{
		await _carouselItemRepository.UpdateAsync(carouselItem);
		await _cacheManager.RemoveByPrefixAsync("NS.OCarouselList.");
	}

	public virtual async Task DeleteOCarouselItemAsync(OCarouselItem carouselItem)
	{
		await _carouselItemRepository.DeleteAsync(carouselItem);
		await _cacheManager.RemoveByPrefixAsync("NS.OCarouselList.");
	}
}
