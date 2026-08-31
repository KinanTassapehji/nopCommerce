using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Stores;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.PrevNextProduct.Models;
using NopStation.Plugin.Widgets.PrevNextProduct.Services;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Components;

public class PrevNextProductViewComponent : NopStationViewComponent
{
	private readonly IPrevNextProductService _prevNextProductService;

	private readonly ILocalizationService _localizationService;

	private readonly IUrlRecordService _urlRecordService;

	private readonly PrevNextProductSettings _prevNextProductSettings;

	private readonly IStaticCacheManager _staticCacheManager;

	private readonly IStoreContext _storeContext;

	private readonly IWorkContext _workContext;

	private readonly IPictureService _pictureService;

	private readonly IWebHelper _webHelper;

	private readonly MediaSettings _mediaSettings;

	public PrevNextProductViewComponent(IPrevNextProductService prevNextProductService, ILocalizationService localizationService, IUrlRecordService urlRecordService, PrevNextProductSettings prevNextProductSettings, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IWorkContext workContext, IPictureService pictureService, IWebHelper webHelper, MediaSettings mediaSettings)
	{
		_prevNextProductService = prevNextProductService;
		_localizationService = localizationService;
		_urlRecordService = urlRecordService;
		_prevNextProductSettings = prevNextProductSettings;
		_staticCacheManager = staticCacheManager;
		_storeContext = storeContext;
		_workContext = workContext;
		_pictureService = pictureService;
		_webHelper = webHelper;
		_mediaSettings = mediaSettings;
	}

	public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
	{
		if (additionalData.GetType() != typeof(ProductDetailsModel))
		{
			return Content("");
		}
		int id = (additionalData as ProductDetailsModel).Id;
		(Product Previous, Product Next) data = await _prevNextProductService.GetProductsAsync(id);
		if (data.Next == null && data.Previous == null)
		{
			return Content("");
		}
		PublicInfoModel model = new PublicInfoModel();
		if (data.Next != null)
		{
			model.Next.HasProduct = true;
			model.Next.Id = data.Next.Id;
			PublicInfoModel.ProductModel next = model.Next;
			next.Name = await _localizationService.GetLocalizedAsync(data.Next, (Product x) => x.Name);
			next = model.Next;
			next.SeName = await _urlRecordService.GetSeNameAsync(data.Next);
			next = model.Next;
			next.Picture = await PrepareProductPictureModelAsync(data.Next, _prevNextProductSettings.ProductThumbnailSize);
			if (_prevNextProductSettings.ProductNameMaxLength > 0 && model.Next.Name.Length > _prevNextProductSettings.ProductNameMaxLength)
			{
				next = model.Next;
				next.ShortName = string.Format(await _localizationService.GetResourceAsync("NopStation.PrevNextProduct.NextProduct.Name"), model.Next.Name.Substring(0, _prevNextProductSettings.ProductNameMaxLength));
			}
		}
		if (data.Previous != null)
		{
			model.Previous.HasProduct = true;
			model.Previous.Id = data.Previous.Id;
			PublicInfoModel.ProductModel next = model.Previous;
			next.Name = await _localizationService.GetLocalizedAsync(data.Previous, (Product x) => x.Name);
			next = model.Previous;
			next.SeName = await _urlRecordService.GetSeNameAsync(data.Previous);
			next = model.Previous;
			next.Picture = await PrepareProductPictureModelAsync(data.Previous, _prevNextProductSettings.ProductThumbnailSize);
			if (_prevNextProductSettings.ProductNameMaxLength > 0 && model.Previous.Name.Length > _prevNextProductSettings.ProductNameMaxLength)
			{
				next = model.Previous;
				next.ShortName = string.Format(await _localizationService.GetResourceAsync("NopStation.PrevNextProduct.PreviousProduct.Name"), model.Previous.Name.Substring(0, _prevNextProductSettings.ProductNameMaxLength));
			}
		}
		return View(model);
	}

	protected virtual async Task<PictureModel> PrepareProductPictureModelAsync(Product product, int? productThumbPictureSize = null)
	{
		if (product == null)
		{
			throw new ArgumentNullException("product");
		}
		string productName = await _localizationService.GetLocalizedAsync(product, (Product x) => x.Name);
		int pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;
		IStaticCacheManager staticCacheManager = _staticCacheManager;
		CacheKey productDetailsPicturesModelKey = NopModelCacheDefaults.ProductDetailsPicturesModelKey;
		object obj = product;
		object obj2 = pictureSize;
		object obj3 = true;
		object obj4 = await _workContext.GetWorkingLanguageAsync();
		object obj5 = _webHelper.IsCurrentConnectionSecured();
		Store store = await _storeContext.GetCurrentStoreAsync();
		CacheKey key = staticCacheManager.PrepareKeyForDefaultCache(productDetailsPicturesModelKey, obj, obj2, obj3, obj4, obj5, store);
		return await _staticCacheManager.GetAsync(key, async delegate
		{
			Picture picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
			(string, Picture) tuple = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
			string imageUrl = tuple.Item1;
			picture = tuple.Item2;
			tuple = await _pictureService.GetPictureUrlAsync(picture);
			string item = tuple.Item1;
			picture = tuple.Item2;
			PictureModel pictureModel = new PictureModel
			{
				ImageUrl = imageUrl,
				FullSizeImageUrl = item
			};
			PictureModel pictureModel2 = pictureModel;
			string title = ((picture == null || string.IsNullOrEmpty(picture.TitleAttribute)) ? string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), productName) : picture.TitleAttribute);
			pictureModel2.Title = title;
			PictureModel pictureModel3 = pictureModel;
			string alternateText = ((picture == null || string.IsNullOrEmpty(picture.AltAttribute)) ? string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), productName) : picture.AltAttribute);
			pictureModel3.AlternateText = alternateText;
			return pictureModel;
		});
	}
}
