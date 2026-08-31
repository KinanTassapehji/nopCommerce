using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Product360View.Factories;
using NopStation.Plugin.Widgets.Product360View.Models;

namespace NopStation.Plugin.Widgets.Product360View.Components;

public class Product360ViewComponent : NopStationViewComponent
{
	private readonly IProduct360ModelFactory _product360ModelFactory;

	private readonly ISettingService _settingService;

	private readonly IStoreContext _storeContext;

	public Product360ViewComponent(IProduct360ModelFactory product360ModelFactory, ISettingService settingService, IStoreContext storeContext)
	{
		_product360ModelFactory = product360ModelFactory;
		_settingService = settingService;
		_storeContext = storeContext;
	}

	public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
	{
		int storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
		if (!(await _settingService.LoadSettingAsync<Product360ViewSettings>(storeId)).IsEnabled)
		{
			return Content("");
		}
		if (additionalData is ProductModel model)
		{
			if (model.Id <= 0 || widgetZone != AdminWidgetZones.ProductDetailsBlock)
			{
				return Content("");
			}
			ImageSetting360Model imageSetting360Model = await _product360ModelFactory.PrepareImageSetting360ModelAsync(model.Id);
			Product360Model product360Model = new Product360Model();
			product360Model.Id = model.Id;
			product360Model.ProductPictureSearchModel.ProductId = model.Id;
			product360Model.ProductPictureSearchModel.SetGridPageSize();
			if (imageSetting360Model != null)
			{
				product360Model.ImageSetting360Model = imageSetting360Model;
			}
			return View("~/Plugins/NopStation.Plugin.Widgets.Product360View/Views/PictureMapping.cshtml", product360Model);
		}
		if (additionalData is ProductDetailsModel detailsModel)
		{
			if (detailsModel.Id <= 0 || widgetZone != PublicWidgetZones.ProductDetailsAfterPictures)
			{
				return Content("");
			}
			Product360Model product360Model2 = await _product360ModelFactory.PrepareImage360DetailsModelAsync(detailsModel.Id);
			if (product360Model2.ImageSetting360Model.IsEnabled)
			{
				product360Model2.Id = detailsModel.Id;
				product360Model2.ProductPictureSearchModel.ProductId = detailsModel.Id;
				return View("~/Plugins/NopStation.Plugin.Widgets.Product360View/Views/Product360View.cshtml", product360Model2);
			}
		}
		return Content("");
	}
}
