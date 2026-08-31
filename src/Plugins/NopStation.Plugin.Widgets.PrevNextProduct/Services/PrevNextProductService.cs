using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Services;

public class PrevNextProductService : IPrevNextProductService
{
	private readonly IRepository<Product> _productRepository;

	private readonly IRepository<ProductCategory> _productCategoryRepository;

	private readonly IRepository<ProductManufacturer> _productManufacturerRepository;

	private readonly PrevNextProductSettings _prevNextProductSettings;

	private readonly IProductService _productService;

	private readonly ICategoryService _categoryService;

	private readonly IManufacturerService _manufacturerService;

	private readonly IRepository<Vendor> _vendorRepository;

	private readonly IWorkContext _workContext;

	private readonly IAclService _aclService;

	private readonly IStoreMappingService _storeMappingService;

	private readonly IStoreContext _storeContext;

	public PrevNextProductService(IRepository<Product> productRepository, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductManufacturer> productManufacturerRepository, PrevNextProductSettings prevNextProductSettings, IProductService productService, ICategoryService categoryService, IManufacturerService manufacturerService, IRepository<Vendor> vendorRepository, IWorkContext workContext, IAclService aclService, IStoreMappingService storeMappingService, IStoreContext storeContext)
	{
		_productRepository = productRepository;
		_productCategoryRepository = productCategoryRepository;
		_productManufacturerRepository = productManufacturerRepository;
		_prevNextProductSettings = prevNextProductSettings;
		_productService = productService;
		_categoryService = categoryService;
		_manufacturerService = manufacturerService;
		_vendorRepository = vendorRepository;
		_workContext = workContext;
		_aclService = aclService;
		_storeMappingService = storeMappingService;
		_storeContext = storeContext;
	}

	protected async Task<IQueryable<Product>> GetProductsQueryAsync(Product product)
	{
		if (_prevNextProductSettings.NavigateBasedOnId == 0)
		{
			IList<ProductCategory> list = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
			if (!list.Any())
			{
				return null;
			}
			int categoryId = list[0].CategoryId;
			IQueryable<Product> outer = _productRepository.Table.Where((Product p) => p.Published && !p.Deleted && p.VisibleIndividually && DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) && DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue));
			var inner = from pc in _productCategoryRepository.Table
				where pc.CategoryId == categoryId
				group pc by pc.ProductId into pc
				select new
				{
					ProductId = pc.Key,
					DisplayOrder = pc.First().DisplayOrder
				};
			return from p in outer
				join pc in inner on p.Id equals pc.ProductId
				orderby pc.DisplayOrder, p.Name
				select p;
		}
		if (_prevNextProductSettings.NavigateBasedOnId == 1)
		{
			IList<ProductManufacturer> list2 = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);
			if (!list2.Any())
			{
				return null;
			}
			int manufacturerId = list2[0].ManufacturerId;
			IQueryable<Product> outer2 = _productRepository.Table.Where((Product p) => p.Published && !p.Deleted && p.VisibleIndividually && DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) && DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue));
			var inner2 = from pm in _productManufacturerRepository.Table
				where pm.ManufacturerId == manufacturerId
				group pm by pm.ProductId into pm
				select new
				{
					ProductId = pm.Key,
					DisplayOrder = pm.First().DisplayOrder
				};
			return from p in outer2
				join pm in inner2 on p.Id equals pm.ProductId
				orderby pm.DisplayOrder, p.Name
				select p;
		}
		if (product.VendorId == 0)
		{
			return null;
		}
		return from p in _productRepository.Table
			join v in _vendorRepository.Table on p.VendorId equals v.Id
			where !v.Deleted && p.Published && !p.Deleted && p.VisibleIndividually && DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) && DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)
			orderby p.DisplayOrder, p.Name
			select p;
	}

	public async Task<(Product Previous, Product Next)> GetProductsAsync(int productId)
	{
		IQueryable<Product> productsQuery = await GetProductsQueryAsync(await _productService.GetProductByIdAsync(productId));
		if (productsQuery == null)
		{
			return (Previous: null, Next: null);
		}
		productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, _storeContext.GetCurrentStore().Id);
		int num = await productsQuery.CountAsync();
		if (num == 0)
		{
			return (Previous: null, Next: null);
		}
		Dictionary<int, int> dictionary = productsQuery.Select((Product x, int index) => new
		{
			Index = index + 1,
			ProductId = x.Id
		}).ToDictionary(t => t.ProductId, t => t.Index);
		Dictionary<int, int> dictionary2 = productsQuery.Select((Product x, int index) => new
		{
			Index = index + 1,
			ProductId = x.Id
		}).ToDictionary(t => t.Index, t => t.ProductId);
		if (!dictionary.ContainsKey(productId))
		{
			return (Previous: null, Next: null);
		}
		int num2 = dictionary[productId];
		int num3 = num2 - 1;
		int num4 = num2 + 1;
		if (num3 == 0 && _prevNextProductSettings.EnableLoop)
		{
			num3 = num;
		}
		if (num4 > num && _prevNextProductSettings.EnableLoop)
		{
			num4 = 1;
		}
		int productId2 = (dictionary2.ContainsKey(num3) ? dictionary2[num3] : 0);
		int nextId = (dictionary2.ContainsKey(num4) ? dictionary2[num4] : 0);
		return (Previous: await _productService.GetProductByIdAsync(productId2), Next: await _productService.GetProductByIdAsync(nextId));
	}
}
