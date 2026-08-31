using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Services.ExportImport.Help;
using Nop.Web.Areas.Admin.Models.Reports;

namespace NopStation.Plugin.Misc.AdminReportExporter.Services;

public class AdminReportExportManager : IAdminReportExportManager
{
	private readonly CatalogSettings _catalogSettings;

	public AdminReportExportManager(CatalogSettings catalogSettings)
	{
		_catalogSettings = catalogSettings;
	}

	public async Task<byte[]> ExportSalesSummaryToXlsxAsync(IEnumerable<SalesSummaryModel> items)
	{
		return await new PropertyManager<SalesSummaryModel>(new PropertyByName<SalesSummaryModel>[6]
		{
			new PropertyByName<SalesSummaryModel>(NopModelHelper.PropertyLabel<SalesSummaryModel>("Summary"), (SalesSummaryModel x, Language l) => x.Summary),
			new PropertyByName<SalesSummaryModel>(NopModelHelper.PropertyLabel<SalesSummaryModel>("NumberOfOrders"), (SalesSummaryModel x, Language l) => x.NumberOfOrders),
			new PropertyByName<SalesSummaryModel>(NopModelHelper.PropertyLabel<SalesSummaryModel>("ProfitStr"), (SalesSummaryModel x, Language l) => x.ProfitStr),
			new PropertyByName<SalesSummaryModel>(NopModelHelper.PropertyLabel<SalesSummaryModel>("Shipping"), (SalesSummaryModel x, Language l) => x.Shipping),
			new PropertyByName<SalesSummaryModel>(NopModelHelper.PropertyLabel<SalesSummaryModel>("Tax"), (SalesSummaryModel x, Language l) => x.Tax),
			new PropertyByName<SalesSummaryModel>(NopModelHelper.PropertyLabel<SalesSummaryModel>("OrderTotal"), (SalesSummaryModel x, Language l) => x.OrderTotal)
		}, _catalogSettings).ExportToXlsxAsync(items);
	}

	public async Task<byte[]> ExportNeverSoldProductsToXlsxAsync(IEnumerable<NeverSoldReportModel> items)
	{
		return await new PropertyManager<NeverSoldReportModel>(new PropertyByName<NeverSoldReportModel>[2]
		{
			new PropertyByName<NeverSoldReportModel>(NopModelHelper.PropertyLabel<NeverSoldReportModel>("ProductId"), (NeverSoldReportModel x, Language l) => x.ProductId),
			new PropertyByName<NeverSoldReportModel>(NopModelHelper.PropertyLabel<NeverSoldReportModel>("ProductName"), (NeverSoldReportModel x, Language l) => x.ProductName)
		}, _catalogSettings).ExportToXlsxAsync(items);
	}

	public async Task<byte[]> ExportLowStockProductsToXlsxAsync(IEnumerable<LowStockProductModel> items)
	{
		return await new PropertyManager<LowStockProductModel>(new PropertyByName<LowStockProductModel>[6]
		{
			new PropertyByName<LowStockProductModel>(NopModelHelper.PropertyLabel<LowStockProductModel>("Id"), (LowStockProductModel x, Language l) => x.Id),
			new PropertyByName<LowStockProductModel>(NopModelHelper.PropertyLabel<LowStockProductModel>("Name"), (LowStockProductModel x, Language l) => x.Name),
			new PropertyByName<LowStockProductModel>(NopModelHelper.PropertyLabel<LowStockProductModel>("Attributes"), (LowStockProductModel x, Language l) => x.Attributes),
			new PropertyByName<LowStockProductModel>(NopModelHelper.PropertyLabel<LowStockProductModel>("ManageInventoryMethod"), (LowStockProductModel x, Language l) => x.ManageInventoryMethod),
			new PropertyByName<LowStockProductModel>(NopModelHelper.PropertyLabel<LowStockProductModel>("StockQuantity"), (LowStockProductModel x, Language l) => x.StockQuantity),
			new PropertyByName<LowStockProductModel>(NopModelHelper.PropertyLabel<LowStockProductModel>("Published"), (LowStockProductModel x, Language l) => x.Published)
		}, _catalogSettings).ExportToXlsxAsync(items);
	}

	public async Task<byte[]> ExportBestsellersToXlsxAsync(IEnumerable<BestsellerModel> items)
	{
		return await new PropertyManager<BestsellerModel>(new PropertyByName<BestsellerModel>[4]
		{
			new PropertyByName<BestsellerModel>(NopModelHelper.PropertyLabel<BestsellerModel>("ProductId"), (BestsellerModel x, Language l) => x.ProductId),
			new PropertyByName<BestsellerModel>(NopModelHelper.PropertyLabel<BestsellerModel>("ProductName"), (BestsellerModel x, Language l) => x.ProductName),
			new PropertyByName<BestsellerModel>(NopModelHelper.PropertyLabel<BestsellerModel>("TotalQuantity"), (BestsellerModel x, Language l) => x.TotalQuantity),
			new PropertyByName<BestsellerModel>(NopModelHelper.PropertyLabel<BestsellerModel>("TotalAmount"), (BestsellerModel x, Language l) => x.TotalAmount)
		}, _catalogSettings).ExportToXlsxAsync(items);
	}

	public async Task<byte[]> ExportCountrySalesToXlsxAsync(IEnumerable<CountryReportModel> items)
	{
		return await new PropertyManager<CountryReportModel>(new PropertyByName<CountryReportModel>[3]
		{
			new PropertyByName<CountryReportModel>(NopModelHelper.PropertyLabel<CountryReportModel>("CountryName"), (CountryReportModel x, Language l) => x.CountryName),
			new PropertyByName<CountryReportModel>(NopModelHelper.PropertyLabel<CountryReportModel>("TotalOrders"), (CountryReportModel x, Language l) => x.TotalOrders),
			new PropertyByName<CountryReportModel>(NopModelHelper.PropertyLabel<CountryReportModel>("SumOrders"), (CountryReportModel x, Language l) => x.SumOrders)
		}, _catalogSettings).ExportToXlsxAsync(items);
	}

	public async Task<byte[]> ExportRegisteredCustomersReportToXlsxAsync(IEnumerable<RegisteredCustomersReportModel> items)
	{
		return await new PropertyManager<RegisteredCustomersReportModel>(new PropertyByName<RegisteredCustomersReportModel>[2]
		{
			new PropertyByName<RegisteredCustomersReportModel>(NopModelHelper.PropertyLabel<RegisteredCustomersReportModel>("Period"), (RegisteredCustomersReportModel x, Language l) => x.Period),
			new PropertyByName<RegisteredCustomersReportModel>(NopModelHelper.PropertyLabel<RegisteredCustomersReportModel>("Customers"), (RegisteredCustomersReportModel x, Language l) => x.Customers)
		}, _catalogSettings).ExportToXlsxAsync(items);
	}

	public async Task<byte[]> ExportBestCustomersReportToXlsxAsync(IEnumerable<BestCustomersReportModel> items)
	{
		return await new PropertyManager<BestCustomersReportModel>(new PropertyByName<BestCustomersReportModel>[4]
		{
			new PropertyByName<BestCustomersReportModel>(NopModelHelper.PropertyLabel<BestCustomersReportModel>("CustomerId"), (BestCustomersReportModel x, Language l) => x.CustomerId),
			new PropertyByName<BestCustomersReportModel>(NopModelHelper.PropertyLabel<BestCustomersReportModel>("CustomerName"), (BestCustomersReportModel x, Language l) => x.CustomerName),
			new PropertyByName<BestCustomersReportModel>(NopModelHelper.PropertyLabel<BestCustomersReportModel>("OrderCount"), (BestCustomersReportModel x, Language l) => x.OrderCount),
			new PropertyByName<BestCustomersReportModel>(NopModelHelper.PropertyLabel<BestCustomersReportModel>("OrderTotal"), (BestCustomersReportModel x, Language l) => x.OrderTotal)
		}, _catalogSettings).ExportToXlsxAsync(items);
	}
}
