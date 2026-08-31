using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Orders;
using Nop.Data;

namespace NopStation.Plugin.Widgets.ProductRibbon.Services;

public class BestSellerService : IBestSellerService
{
	private readonly IRepository<Order> _orderRepository;

	private readonly IRepository<OrderItem> _orderItemRepository;

	private readonly ProductRibbonSettings _productRibbonSettings;

	private readonly IStoreContext _storeContext;

	private readonly IStaticCacheManager _cacheManager;

	public BestSellerService(IRepository<Order> orderRepository, IRepository<OrderItem> orderItemRepository, ProductRibbonSettings productRibbonSettings, IStoreContext storeContext, IStaticCacheManager cacheManager)
	{
		_orderRepository = orderRepository;
		_orderItemRepository = orderItemRepository;
		_productRibbonSettings = productRibbonSettings;
		_storeContext = storeContext;
		_cacheManager = cacheManager;
	}

	public virtual async Task<BestsellersReportLine> BestSellerReportAsync(int productId)
	{
		int num = (_productRibbonSettings.BestSellStoreWise ? (await _storeContext.GetCurrentStoreAsync()).Id : 0);
		int storeId = num;
		List<int> sids = _productRibbonSettings.BestSellShippingStatusIds ?? new List<int>();
		List<int> pids = _productRibbonSettings.BestSellPaymentStatusIds ?? new List<int>();
		List<int> oids = _productRibbonSettings.BestSellOrderStatusIds ?? new List<int>();
		int days = _productRibbonSettings.SoldInDays;
		CacheKey key = _cacheManager.PrepareKeyForDefaultCache(ProductRibbonDefaults.BestSellerKey, productId, storeId, sids, pids, oids, days);
		return await _cacheManager.GetAsync(key, delegate
		{
			DateTime createdFromUtc = DateTime.UtcNow.AddDays(-days);
			return (from orderItem in _orderItemRepository.Table
				join o in _orderRepository.Table on orderItem.OrderId equals o.Id
				where (storeId == 0 || storeId == o.StoreId) && createdFromUtc <= o.CreatedOnUtc && (!sids.Any() || sids.Contains(o.ShippingStatusId)) && (!pids.Any() || pids.Contains(o.PaymentStatusId)) && (!oids.Any() || oids.Contains(o.OrderStatusId)) && orderItem.ProductId == productId
				select orderItem into orderItem
				group orderItem by orderItem.ProductId into g
				select new BestsellersReportLine
				{
					ProductId = g.Key,
					TotalAmount = g.Sum((OrderItem x) => x.PriceExclTax),
					TotalQuantity = g.Sum((OrderItem x) => x.Quantity)
				}).FirstOrDefault();
		});
	}
}
