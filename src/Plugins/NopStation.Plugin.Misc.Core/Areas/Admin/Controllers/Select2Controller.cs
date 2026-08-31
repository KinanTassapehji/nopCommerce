using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Controllers;

public class Select2Controller : NopStationAdminController
{
	private readonly ICategoryService _categoryService;

	private readonly IProductService _productService;

	private readonly IManufacturerService _manufacturerService;

	private readonly INopStationCustomerService _customerService;

	public Select2Controller(ICategoryService categoryService, IProductService productService, IManufacturerService manufacturerService, INopStationCustomerService customerService)
	{
		_categoryService = categoryService;
		_productService = productService;
		_manufacturerService = manufacturerService;
		_customerService = customerService;
	}

	public async Task<IActionResult> Products(string q, int page = 1)
	{
		IPagedList<Product> pagedList = await _productService.SearchProductsAsync(page - 1, 10, null, null, 0, 0, 0, null, visibleIndividuallyOnly: false, excludeFeaturedProducts: false, null, null, 0, q, searchDescriptions: false, searchManufacturerPartNumber: true, searchSku: true, searchProductTags: false, languageId: 0, orderBy: ProductSortingEnum.Position, showHidden: true);
		Select2ResponseModel select2ResponseModel = new Select2ResponseModel();
		foreach (Product item in pagedList)
		{
			select2ResponseModel.Results.Add(new Select2ResponseModel.Select2Item
			{
				Id = item.Id,
				Text = item.Name
			});
		}
		select2ResponseModel.Pagination.More = pagedList.HasNextPage;
		return Json(select2ResponseModel);
	}

	public async Task<IActionResult> Categories(string q, int page = 1)
	{
		IPagedList<Category> categories = await _categoryService.GetAllCategoriesAsync(q, 0, page - 1, 10, showHidden: true);
		Select2ResponseModel response = new Select2ResponseModel();
		foreach (Category item in categories)
		{
			IList<Select2ResponseModel.Select2Item> results = response.Results;
			Select2ResponseModel.Select2Item select2Item = new Select2ResponseModel.Select2Item
			{
				Id = item.Id
			};
			Select2ResponseModel.Select2Item select2Item2 = select2Item;
			select2Item2.Text = await _categoryService.GetFormattedBreadCrumbAsync(item);
			results.Add(select2Item);
		}
		response.Pagination.More = categories.HasNextPage;
		return Json(response);
	}

	public async Task<IActionResult> Manufacturers(string q, int page = 1)
	{
		IPagedList<Manufacturer> pagedList = await _manufacturerService.GetAllManufacturersAsync(q, 0, page - 1, 10, showHidden: true);
		Select2ResponseModel select2ResponseModel = new Select2ResponseModel();
		foreach (Manufacturer item in pagedList)
		{
			select2ResponseModel.Results.Add(new Select2ResponseModel.Select2Item
			{
				Id = item.Id,
				Text = item.Name
			});
		}
		select2ResponseModel.Pagination.More = pagedList.HasNextPage;
		return Json(select2ResponseModel);
	}

	public async Task<IActionResult> Customers(string q, int page = 1)
	{
		IPagedList<Customer> customers = await _customerService.GetCustomersAsync(q, showHidden: true, page - 1, 10);
		Select2ResponseModel response = new Select2ResponseModel();
		foreach (Customer item in customers)
		{
			IList<Select2ResponseModel.Select2Item> results = response.Results;
			Select2ResponseModel.Select2Item select2Item = new Select2ResponseModel.Select2Item
			{
				Id = item.Id
			};
			Select2ResponseModel.Select2Item select2Item2 = select2Item;
			select2Item2.Text = await _customerService.FormatCustomerNameAsync(item);
			results.Add(select2Item);
		}
		response.Pagination.More = customers.HasNextPage;
		return Json(response);
	}
}
