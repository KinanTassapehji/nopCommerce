using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.Product360View.Domain;
using NopStation.Plugin.Widgets.Product360View.Models;
using NopStation.Plugin.Widgets.Product360View.Services;

namespace NopStation.Plugin.Widgets.Product360View.Factories;

public class Product360ModelFactory : IProduct360ModelFactory
{
	private readonly IProductService _productService;

	private readonly IProductPictureMappingService _productPictureMappingService;

	private readonly IPictureService _pictureService;

	private readonly IProductImageSettingService _productImageSettingService;

	public Product360ModelFactory(IProductService productService, IProductPictureMappingService productPictureMappingService, IPictureService pictureService, IProductImageSettingService productImageSettingService)
	{
		_productService = productService;
		_productPictureMappingService = productPictureMappingService;
		_pictureService = pictureService;
		_productImageSettingService = productImageSettingService;
	}

	public virtual async Task<Product360PictureListModel> PrepareProduct360PictureListModelAsync(Picture360SearchModel searchModel, Product product)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		ArgumentNullException.ThrowIfNull(product, "product");
		IPagedList<ProductPictureMapping360> productPictures = (await _productPictureMappingService.GetPictureMappingsByProductIdAsync(product.Id, searchModel.IsPanorama)).ToPagedList(searchModel);
		return await new Product360PictureListModel().PrepareToGridAsync(searchModel, productPictures, () => productPictures.SelectAwait<ProductPictureMapping360, ProductPicture360Model>(async delegate(ProductPictureMapping360 productPicture)
		{
			ProductPicture360Model product360PictureModel = new ProductPicture360Model
			{
				Id = productPicture.Id,
				ProductId = productPicture.ProductId,
				PictureId = productPicture.PictureId,
				DisplayOrder = productPicture.DisplayOrder,
				IsPanorama = productPicture.IsPanorama
			};
			Picture picture = (await _pictureService.GetPictureByIdAsync(productPicture.PictureId)) ?? throw new Exception("Picture cannot be loaded");
			ProductPicture360Model productPicture360Model = product360PictureModel;
			productPicture360Model.PictureUrl = (await _pictureService.GetPictureUrlAsync(picture)).Item1;
			product360PictureModel.OverrideAltAttribute = picture.AltAttribute;
			product360PictureModel.OverrideTitleAttribute = picture.TitleAttribute;
			return product360PictureModel;
		}));
	}

	public async Task<ImageSetting360Model> PrepareImageSetting360ModelAsync(int productId)
	{
		return (await _productImageSettingService.GetImageSettingByProductIdAsync(productId))?.ToModel<ImageSetting360Model>();
	}

	public async Task<Product360Model> PrepareImage360DetailsModelAsync(int productId)
	{
		if (await _productService.GetProductByIdAsync(productId) == null)
		{
			throw new ArgumentNullException("product");
		}
		ProductImageSetting360 imageSetting = await _productImageSettingService.GetImageSettingByProductIdAsync(productId);
		IList<Picture> pictures = await _productPictureMappingService.Get360PicturesByProductIdAsync(productId);
		List<string> pictureUrls = new List<string>();
		for (int i = 0; i < pictures?.Count; i++)
		{
			Picture picture = pictures[i];
			(string, Picture) obj = await _pictureService.GetPictureUrlAsync(picture);
			var (item, _) = obj;
			_ = obj.Item2;
			pictureUrls.Add(item);
		}
		IList<Picture> panoramaPictures = await _productPictureMappingService.Get360PicturesByProductIdAsync(productId, isPanorama: true);
		List<string> panoramaPictureUrls = new List<string>();
		for (int i = 0; i < panoramaPictures?.Count; i++)
		{
			Picture picture2 = panoramaPictures[i];
			(string, Picture) obj2 = await _pictureService.GetPictureUrlAsync(picture2);
			var (item2, _) = obj2;
			_ = obj2.Item2;
			panoramaPictureUrls.Add(item2);
		}
		Product360Model product360Model = new Product360Model();
		if (imageSetting != null)
		{
			product360Model.ImageSetting360Model = imageSetting.ToModel<ImageSetting360Model>();
		}
		product360Model.PictureUrls = pictureUrls;
		product360Model.PanoramaPictureUrls = panoramaPictureUrls;
		return product360Model;
	}
}
