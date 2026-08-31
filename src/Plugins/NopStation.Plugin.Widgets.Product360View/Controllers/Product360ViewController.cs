using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Widgets.Product360View.Domain;
using NopStation.Plugin.Widgets.Product360View.Factories;
using NopStation.Plugin.Widgets.Product360View.Infrastructure;
using NopStation.Plugin.Widgets.Product360View.Models;
using NopStation.Plugin.Widgets.Product360View.Services;

namespace NopStation.Plugin.Widgets.Product360View.Controllers;

public class Product360ViewController : NopStationAdminController
{
	private readonly ISettingHelper<Product360ViewSettings, ConfigurationModel> _settingHelper;

	private readonly IProductService _productService;

	private readonly IPictureService _pictureService;

	private readonly IWorkContext _workContext;

	private readonly IProductPictureMappingService _productPictureMappingService;

	private readonly IProduct360ModelFactory _product360ModelFactory;

	private readonly IProductImageSettingService _productImageSettingService;

	private readonly IStaticCacheManager _staticCacheManager;

	private readonly ILocalizationService _localizationService;

	private readonly IStoreContext _storeContext;

	private readonly ISettingService _settingService;

	private readonly INotificationService _notificationService;

	public Product360ViewController(ISettingHelper<Product360ViewSettings, ConfigurationModel> settingHelper, IProductService productService, IPictureService pictureService, IWorkContext workContext, IProductPictureMappingService productPictureMappingService, IProduct360ModelFactory product360ModelFactory, IProductImageSettingService productImageSettingService, IStaticCacheManager staticCacheManager, ILocalizationService localizationService, IStoreContext storeContext, ISettingService settingService, INotificationService notificationService)
	{
		_settingHelper = settingHelper;
		_productService = productService;
		_pictureService = pictureService;
		_workContext = workContext;
		_productPictureMappingService = productPictureMappingService;
		_product360ModelFactory = product360ModelFactory;
		_productImageSettingService = productImageSettingService;
		_staticCacheManager = staticCacheManager;
		_localizationService = localizationService;
		_storeContext = storeContext;
		_settingService = settingService;
		_notificationService = notificationService;
	}

	[CheckPermission("ManageNopStationProduct360View", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure()
	{
		return View("~/Plugins/NopStation.Plugin.Widgets.Product360View/Views/Configure.cshtml", await _settingHelper.PrepareConfigurationModelAsync(null));
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationProduct360View", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> Configure(ConfigurationModel model)
	{
		await _settingHelper.SaveConfigurationModelAsync(model, null, true);
		return RedirectToAction("Configure");
	}

	[HttpPost]
	[IgnoreAntiforgeryToken]
	[CheckPermission("Catalog.ProductsCreateEditDelete", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Product360PictureAdd(int productId, IFormCollection form)
	{
		if (productId == 0)
		{
			throw new ArgumentException();
		}
		Product product = (await _productService.GetProductByIdAsync(productId)) ?? throw new ArgumentException("No product found with the specified id");
		List<IFormFile> files = form.Files.ToList();
		if (files.Count == 0)
		{
			return Json(new
			{
				success = false
			});
		}
		Vendor vendor = await _workContext.GetCurrentVendorAsync();
		if (vendor != null && product.VendorId != vendor.Id)
		{
			return RedirectToAction("List", "Product");
		}
		try
		{
			int lastDisplayOrder = await _productPictureMappingService.GetLastPictureOrderByProductIdAsync(productId);
			foreach (IFormFile item in files)
			{
				Picture picture = await _pictureService.InsertPictureAsync(item);
				IPictureService pictureService = _pictureService;
				int id = picture.Id;
				await pictureService.SetSeoFilenameAsync(id, await _pictureService.GetPictureSeNameAsync(product.Name));
				lastDisplayOrder++;
				await _productPictureMappingService.InsertPictureMappingAsync(new ProductPictureMapping360
				{
					PictureId = picture.Id,
					ProductId = product.Id,
					DisplayOrder = lastDisplayOrder,
					IsPanorama = false
				});
			}
		}
		catch (Exception ex)
		{
			return Json(new
			{
				success = false,
				message = await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Pictures.Alert.PictureAdd") + " " + ex.Message
			});
		}
		return Json(new
		{
			success = true
		});
	}

	[HttpPost]
	[IgnoreAntiforgeryToken]
	[CheckPermission("Catalog.ProductsCreateEditDelete", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> PanoramaPictureAdd(int productId, IFormCollection form)
	{
		if (productId == 0)
		{
			throw new ArgumentException();
		}
		Product product = (await _productService.GetProductByIdAsync(productId)) ?? throw new ArgumentException("No product found with the specified id");
		List<IFormFile> files = form.Files.ToList();
		if (files.Count == 0)
		{
			return Json(new
			{
				success = false
			});
		}
		Vendor vendor = await _workContext.GetCurrentVendorAsync();
		if (vendor != null && product.VendorId != vendor.Id)
		{
			return RedirectToAction("List", "Product");
		}
		try
		{
			int lastDisplayOrder = await _productPictureMappingService.GetLastPictureOrderByProductIdAsync(productId, isPanorama: true);
			foreach (IFormFile item in files)
			{
				Picture picture = await _pictureService.InsertPictureAsync(item);
				IPictureService pictureService = _pictureService;
				int id = picture.Id;
				await pictureService.SetSeoFilenameAsync(id, await _pictureService.GetPictureSeNameAsync(product.Name));
				lastDisplayOrder++;
				await _productPictureMappingService.InsertPictureMappingAsync(new ProductPictureMapping360
				{
					PictureId = picture.Id,
					ProductId = product.Id,
					DisplayOrder = lastDisplayOrder,
					IsPanorama = true
				});
			}
		}
		catch (Exception ex)
		{
			return Json(new
			{
				success = false,
				message = await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Pictures.Alert.PictureAdd") + " " + ex.Message
			});
		}
		return Json(new
		{
			success = true
		});
	}

	[HttpPost]
	[CheckPermission("Catalog.ProductsCreateEditDelete", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Product360PictureList(Picture360SearchModel searchModel)
	{
		Product product = (await _productService.GetProductByIdAsync(searchModel.ProductId)) ?? throw new ArgumentException("No product found with the specified id");
		Vendor vendor = await _workContext.GetCurrentVendorAsync();
		if (vendor != null && product.VendorId != vendor.Id)
		{
			return Content("This is not your product");
		}
		return Json(await _product360ModelFactory.PrepareProduct360PictureListModelAsync(searchModel, product));
	}

	[HttpPost]
	[CheckPermission("Catalog.ProductsCreateEditDelete", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Product360PictureUpdate(ProductPicture360Model model)
	{
		ProductPictureMapping360 productPicture = (await _productPictureMappingService.GetPictureMappingByIdAsync(model.Id)) ?? throw new ArgumentException("No product 360 picture found with the specified id");
		Vendor currentVendor = await _workContext.GetCurrentVendorAsync();
		if (currentVendor != null)
		{
			Product product = await _productService.GetProductByIdAsync(productPicture.ProductId);
			if (product != null && product.VendorId != currentVendor.Id)
			{
				return Content("This is not your product");
			}
		}
		Picture picture = (await _pictureService.GetPictureByIdAsync(productPicture.PictureId)) ?? throw new ArgumentException("No picture found with the specified id");
		await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.Picture360Prefix, productPicture.ProductId);
		IPictureService pictureService = _pictureService;
		int id = picture.Id;
		await pictureService.UpdatePictureAsync(id, await _pictureService.LoadPictureBinaryAsync(picture), picture.MimeType, picture.SeoFilename, model.OverrideAltAttribute, model.OverrideTitleAttribute);
		productPicture.DisplayOrder = model.DisplayOrder;
		await _productPictureMappingService.UpdatePictureMappingAsync(productPicture);
		return new NullJsonResult();
	}

	[HttpPost]
	[CheckPermission("Catalog.ProductsCreateEditDelete", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Product360PictureDelete(int id)
	{
		ProductPictureMapping360 productPicture = (await _productPictureMappingService.GetPictureMappingByIdAsync(id)) ?? throw new ArgumentException("No product 360 picture found with the specified id");
		Vendor currentVendor = await _workContext.GetCurrentVendorAsync();
		if (currentVendor != null)
		{
			Product product = await _productService.GetProductByIdAsync(productPicture.ProductId);
			if (product != null && product.VendorId != currentVendor.Id)
			{
				return Content("This is not your product");
			}
		}
		int pictureId = productPicture.PictureId;
		await _productPictureMappingService.DeletePictureMappingAsync(productPicture);
		Picture picture = (await _pictureService.GetPictureByIdAsync(pictureId)) ?? throw new ArgumentException("No picture found with the specified id");
		await _pictureService.DeletePictureAsync(picture);
		return new NullJsonResult();
	}

	[HttpPost]
	[CheckPermission("Catalog.ProductsCreateEditDelete", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> ProductImageSettingAddOrUpdate(ImageSetting360Model model)
	{
		ArgumentNullException.ThrowIfNull(model, "model");
		Vendor currentVendor = await _workContext.GetCurrentVendorAsync();
		if (currentVendor != null)
		{
			Product product = await _productService.GetProductByIdAsync(model.ProductId);
			if (product != null && product.VendorId != currentVendor.Id)
			{
				return Content("This is not your product");
			}
		}
		await _productImageSettingService.AddOrUpdateImageSettingAsync(model);
		return new NullJsonResult();
	}
}
