using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Reports;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.AdminReportExporter.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Misc.AdminReportExporter.Areas.Admin.Controllers;

public class AdminExportController : NopStationAdminController
{
	private readonly IAdminReportExportManager _exportManager;

	private readonly IReportModelFactory _reportModelFactory;

	public AdminExportController(IAdminReportExportManager exportManager, IReportModelFactory reportModelFactory)
	{
		_exportManager = exportManager;
		_reportModelFactory = reportModelFactory;
	}

	[HttpPost]
	[CheckPermission("ManageAdminReportExporter", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> SalesSummary(SalesSummarySearchModel searchModel)
	{
		searchModel.SetGridPageSize(int.MaxValue);
		SalesSummaryListModel salesSummaryListModel = await _reportModelFactory.PrepareSalesSummaryListModelAsync(searchModel);
		return File(await _exportManager.ExportSalesSummaryToXlsxAsync(salesSummaryListModel.Data), MimeTypes.TextXlsx, "SalesSummary.xlsx");
	}

	[HttpPost]
	[CheckPermission("ManageAdminReportExporter", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> LowStock(LowStockProductSearchModel searchModel)
	{
		searchModel.SetGridPageSize(int.MaxValue);
		LowStockProductListModel lowStockProductListModel = await _reportModelFactory.PrepareLowStockProductListModelAsync(searchModel);
		return File(await _exportManager.ExportLowStockProductsToXlsxAsync(lowStockProductListModel.Data), MimeTypes.TextXlsx, "LowStock.xlsx");
	}

	[HttpPost]
	[CheckPermission("ManageAdminReportExporter", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Bestsellers(BestsellerSearchModel searchModel)
	{
		searchModel.SetGridPageSize(int.MaxValue);
		BestsellerListModel bestsellerListModel = await _reportModelFactory.PrepareBestsellerListModelAsync(searchModel);
		return File(await _exportManager.ExportBestsellersToXlsxAsync(bestsellerListModel.Data), MimeTypes.TextXlsx, "Bestsellers.xlsx");
	}

	[HttpPost]
	[CheckPermission("ManageAdminReportExporter", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> NeverSold(NeverSoldReportSearchModel searchModel)
	{
		searchModel.SetGridPageSize(int.MaxValue);
		NeverSoldReportListModel neverSoldReportListModel = await _reportModelFactory.PrepareNeverSoldListModelAsync(searchModel);
		return File(await _exportManager.ExportNeverSoldProductsToXlsxAsync(neverSoldReportListModel.Data), MimeTypes.TextXlsx, "NeverSold.xlsx");
	}

	[HttpPost]
	[CheckPermission("ManageAdminReportExporter", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> CountrySales(CountryReportSearchModel searchModel)
	{
		searchModel.SetGridPageSize(int.MaxValue);
		CountryReportListModel countryReportListModel = await _reportModelFactory.PrepareCountrySalesListModelAsync(searchModel);
		return File(await _exportManager.ExportCountrySalesToXlsxAsync(countryReportListModel.Data), MimeTypes.TextXlsx, "CountrySales.xlsx");
	}

	[HttpPost]
	[CheckPermission("ManageAdminReportExporter", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> RegisteredCustomers(RegisteredCustomersReportSearchModel searchModel)
	{
		searchModel.SetGridPageSize(int.MaxValue);
		RegisteredCustomersReportListModel registeredCustomersReportListModel = await _reportModelFactory.PrepareRegisteredCustomersReportListModelAsync(searchModel);
		return File(await _exportManager.ExportRegisteredCustomersReportToXlsxAsync(registeredCustomersReportListModel.Data), MimeTypes.TextXlsx, "RegisteredCustomers.xlsx");
	}

	[HttpPost]
	[CheckPermission("ManageAdminReportExporter", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> BestCustomersByOrderTotal(CustomerReportsSearchModel searchModel)
	{
		searchModel.SetGridPageSize(int.MaxValue);
		searchModel.BestCustomersByOrderTotal.OrderBy = OrderByEnum.OrderByTotalAmount;
		BestCustomersReportListModel bestCustomersReportListModel = await _reportModelFactory.PrepareBestCustomersReportListModelAsync(searchModel.BestCustomersByOrderTotal);
		return File(await _exportManager.ExportBestCustomersReportToXlsxAsync(bestCustomersReportListModel.Data), MimeTypes.TextXlsx, "BestCustomersByOrderTotal.xlsx");
	}

	[HttpPost]
	[CheckPermission("ManageAdminReportExporter", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> BestCustomersByNumberOfOrders(CustomerReportsSearchModel searchModel)
	{
		searchModel.SetGridPageSize(int.MaxValue);
		searchModel.BestCustomersByNumberOfOrders.OrderBy = OrderByEnum.OrderByQuantity;
		BestCustomersReportListModel bestCustomersReportListModel = await _reportModelFactory.PrepareBestCustomersReportListModelAsync(searchModel.BestCustomersByNumberOfOrders);
		return File(await _exportManager.ExportBestCustomersReportToXlsxAsync(bestCustomersReportListModel.Data), MimeTypes.TextXlsx, "BestCustomersByNumberOfOrders.xlsx");
	}
}
