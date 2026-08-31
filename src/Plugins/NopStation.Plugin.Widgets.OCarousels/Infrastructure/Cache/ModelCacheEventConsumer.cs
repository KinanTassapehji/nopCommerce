using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.OCarousels.Domains;

namespace NopStation.Plugin.Widgets.OCarousels.Infrastructure.Cache;

public class ModelCacheEventConsumer : IConsumer<EntityInsertedEvent<Manufacturer>>, IConsumer<EntityUpdatedEvent<Manufacturer>>, IConsumer<EntityDeletedEvent<Manufacturer>>, IConsumer<EntityInsertedEvent<Category>>, IConsumer<EntityUpdatedEvent<Category>>, IConsumer<EntityDeletedEvent<Category>>, IConsumer<EntityInsertedEvent<OCarousel>>, IConsumer<EntityUpdatedEvent<OCarousel>>, IConsumer<EntityDeletedEvent<OCarousel>>, IConsumer<EntityInsertedEvent<OCarouselItem>>, IConsumer<EntityUpdatedEvent<OCarouselItem>>, IConsumer<EntityDeletedEvent<OCarouselItem>>
{
	public static CacheKey OCAROUSEL_BACKGROUND_PICTURE_MODEL_KEY = new CacheKey("Nopstation.ocarousel.items.background_picture.{0}-{1}");

	public const string OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY = "Nopstation.ocarousel.items.background_picture.";

	public static CacheKey OCAROUSEL_CATEGORIES_MODEL_KEY = new CacheKey("Nopstation.ocarousel.items.categories.{0}-{1}-{2}-{3}");

	public const string OCAROUSEL_CATEGORIES_PATTERN_KEY = "Nopstation.ocarousel.items.categories.";

	public static CacheKey OCAROUSEL_MANUFACTURERS_MODEL_KEY = new CacheKey("Nopstation.ocarousel.items.manufacturers.{0}-{1}-{2}-{3}");

	public const string OCAROUSEL_MANUFACTURERS_PATTERN_KEY = "Nopstation.ocarousel.items.manufacturers.";

	public static CacheKey OCAROUSEL_CUSTOMRODUCTIDS_MODEL_KEY = new CacheKey("Nopstation.ocarousel.items.customproductids.{0}");

	public const string OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY = "Nopstation.ocarousel.items.customproductids.";

	private readonly IStaticCacheManager _cacheManager;

	public ModelCacheEventConsumer(IStaticCacheManager cacheManager)
	{
		_cacheManager = cacheManager;
	}

	public async Task HandleEventAsync(EntityInsertedEvent<Manufacturer> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.manufacturers.");
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<Manufacturer> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.manufacturers.");
	}

	public async Task HandleEventAsync(EntityDeletedEvent<Manufacturer> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.manufacturers.");
	}

	public async Task HandleEventAsync(EntityInsertedEvent<Category> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.categories.");
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.categories.");
	}

	public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.categories.");
	}

	public async Task HandleEventAsync(EntityInsertedEvent<OCarousel> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.customproductids.");
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.background_picture.");
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<OCarousel> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.customproductids.");
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.background_picture.");
	}

	public async Task HandleEventAsync(EntityDeletedEvent<OCarousel> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.customproductids.");
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.background_picture.");
	}

	public async Task HandleEventAsync(EntityInsertedEvent<OCarouselItem> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.customproductids.");
	}

	public async Task HandleEventAsync(EntityUpdatedEvent<OCarouselItem> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.customproductids.");
	}

	public async Task HandleEventAsync(EntityDeletedEvent<OCarouselItem> eventMessage)
	{
		await _cacheManager.RemoveByPrefixAsync("Nopstation.ocarousel.items.customproductids.");
	}
}
