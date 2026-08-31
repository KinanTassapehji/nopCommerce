using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Data;
using NopStation.Plugin.Widgets.ProductTabs.Domains;

namespace NopStation.Plugin.Widgets.ProductTabs.Services;

public class ProductTabService : IProductTabService
{
	private readonly IStaticCacheManager _cacheManager;

	private readonly IRepository<ProductTab> _productTabRepository;

	private readonly IRepository<ProductTabItem> _productTabItemRepository;

	private readonly IRepository<ProductTabItemProduct> _productTabItemProductRepository;

	private readonly IRepository<StoreMapping> _storeMappingRepository;

	private readonly CatalogSettings _catalogSettings;

	private readonly IEventPublisher _eventPublisher;

	public ProductTabService(IStaticCacheManager cacheManager, IRepository<ProductTab> productTabRepository, IRepository<ProductTabItem> productTabItemRepository, IRepository<ProductTabItemProduct> productTabItemProductRepository, IRepository<StoreMapping> storeMappingRepository, CatalogSettings catalogSettings, IEventPublisher eventPublisher)
	{
		_cacheManager = cacheManager;
		_productTabRepository = productTabRepository;
		_productTabItemRepository = productTabItemRepository;
		_productTabItemProductRepository = productTabItemProductRepository;
		_storeMappingRepository = storeMappingRepository;
		_catalogSettings = catalogSettings;
		_eventPublisher = eventPublisher;
	}

	public async Task DeleteProductTabAsync(ProductTab productTab)
	{
		await _productTabRepository.DeleteAsync(productTab);
	}

	public async Task InsertProductTabAsync(ProductTab productTab)
	{
		await _productTabRepository.InsertAsync(productTab);
	}

	public async Task UpdateProductTabAsync(ProductTab productTab)
	{
		await _productTabRepository.UpdateAsync(productTab);
	}

	public async Task<ProductTab> GetProductTabByIdAsync(int productTabId)
	{
		if (productTabId == 0)
		{
			return null;
		}
		return await _productTabRepository.GetByIdAsync(productTabId, (ICacheKeyService cache) => (CacheKey)null);
	}

	public async Task<IPagedList<ProductTab>> GetAllProductTabsAsync(List<int> widgetZoneIds = null, bool hasItemsOnly = false, int storeId = 0, bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		IQueryable<ProductTab> source = _productTabRepository.Table;
		if (widgetZoneIds != null && widgetZoneIds.Any())
		{
			source = source.Where((ProductTab productTab) => widgetZoneIds.Contains(productTab.WidgetZoneId));
		}
		if (active.HasValue)
		{
			source = source.Where((ProductTab productTab) => productTab.Active == ((bool?)active).Value);
		}
		if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
		{
			List<StoreMapping> sm = _storeMappingRepository.Table.Where((StoreMapping x) => x.EntityName == "ProductTab" && x.StoreId == storeId).ToList();
			source = source.Where((ProductTab x) => !x.LimitedToStores || sm.Any((StoreMapping y) => y.EntityId == x.Id));
		}
		source = source.OrderBy((ProductTab e) => e.DisplayOrder);
		return await source.ToPagedListAsync(pageIndex, pageSize);
	}

	public async Task DeleteProductTabItemAsync(ProductTabItem productTabItem)
	{
		await _productTabItemRepository.DeleteAsync(productTabItem);
	}

	public async Task UpdateProductTabItemAsync(ProductTabItem productTabItem)
	{
		await _productTabItemRepository.UpdateAsync(productTabItem);
	}

	public async Task<ProductTabItemProduct> GetProductTabItemProductByIdAsync(int productTabItemProductId)
	{
		if (productTabItemProductId == 0)
		{
			return null;
		}
		return await _productTabItemProductRepository.GetByIdAsync(productTabItemProductId, (ICacheKeyService cache) => (CacheKey)null);
	}

	public async Task DeleteProductTabItemProductAsync(ProductTabItemProduct productTabItemProduct)
	{
		await _productTabItemProductRepository.DeleteAsync(productTabItemProduct);
	}

	public async Task UpdateProductTabItemProductAsync(ProductTabItemProduct productTabItemProduct)
	{
		await _productTabItemProductRepository.UpdateAsync(productTabItemProduct);
	}

	public async Task<ProductTabItem> GetProductTabItemByIdAsync(int productTabItemId)
	{
		if (productTabItemId == 0)
		{
			return null;
		}
		return await _productTabItemRepository.GetByIdAsync(productTabItemId, (ICacheKeyService cache) => (CacheKey)null);
	}

	public List<ProductTabItem> GetProductTabItemsByProductTabId(int productTabId)
	{
		if (productTabId == 0)
		{
			return null;
		}
		return _productTabItemRepository.Table.Where((ProductTabItem x) => x.ProductTabId == productTabId).ToList();
	}

	public List<ProductTabItemProduct> GetProductTabItemProductsByProductTabItemId(int productTabItemId)
	{
		if (productTabItemId == 0)
		{
			return null;
		}
		return _productTabItemProductRepository.Table.Where((ProductTabItemProduct x) => x.ProductTabItemId == productTabItemId).ToList();
	}

	public async Task InsertProductTabItemAsync(ProductTabItem productTabItem)
	{
		await _productTabItemRepository.InsertAsync(productTabItem);
	}

	public async Task InsertProductTabItemProductAsync(ProductTabItemProduct productTabItemProduct)
	{
		await _productTabItemProductRepository.InsertAsync(productTabItemProduct);
	}
}
