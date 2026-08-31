using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Widgets.Product360View.Domain;
using NopStation.Plugin.Widgets.Product360View.Infrastructure;
using NopStation.Plugin.Widgets.Product360View.Models;

namespace NopStation.Plugin.Widgets.Product360View.Services;

public class ProductImageSettingService : IProductImageSettingService
{
	private readonly IRepository<ProductImageSetting360> _imageSettingRepository;

	private readonly IStaticCacheManager _staticCacheManager;

	public ProductImageSettingService(IRepository<ProductImageSetting360> imageSettingRepository, IStaticCacheManager staticCacheManager)
	{
		_imageSettingRepository = imageSettingRepository;
		_staticCacheManager = staticCacheManager;
	}

	public virtual async Task<ProductImageSetting360> GetImageSettingByIdAsync(int id)
	{
		IQueryable<ProductImageSetting360> query = _imageSettingRepository.Table;
		CacheKey key = _staticCacheManager.PrepareKeyForDefaultCache(Picture360CacheKeys.ImageSettingCacheKey, id);
		return await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync((ProductImageSetting360 c) => c.Id == id));
	}

	public virtual async Task AddOrUpdateImageSettingAsync(ImageSetting360Model settingModel)
	{
		ArgumentNullException.ThrowIfNull(settingModel, "settingModel");
		if (settingModel.Id > 0)
		{
			ProductImageSetting360 existingSetting = await _imageSettingRepository.GetByIdAsync(settingModel.Id);
			if (existingSetting == null)
			{
				throw new ArgumentNullException("settingModel");
			}
			existingSetting.ProductId = settingModel.ProductId;
			existingSetting.IsEnabled = settingModel.IsEnabled;
			existingSetting.IsLoopEnabled = settingModel.IsLoopEnabled;
			existingSetting.IsPanoramaEnabled = settingModel.IsPanoramaEnabled;
			existingSetting.IsZoomEnabled = settingModel.IsZoomEnabled;
			existingSetting.BehaviorTypeId = settingModel.BehaviorTypeId;
			await _imageSettingRepository.UpdateAsync(existingSetting);
			await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.PictureSetting360Prefix, existingSetting.Id);
			return;
		}
		ProductImageSetting360 oldSetting = await _imageSettingRepository.Table.Where((ProductImageSetting360 c) => c.ProductId == settingModel.ProductId).FirstOrDefaultAsync();
		if (oldSetting != null)
		{
			oldSetting.ProductId = settingModel.ProductId;
			oldSetting.IsEnabled = settingModel.IsEnabled;
			oldSetting.IsLoopEnabled = settingModel.IsLoopEnabled;
			oldSetting.IsPanoramaEnabled = settingModel.IsPanoramaEnabled;
			oldSetting.IsZoomEnabled = settingModel.IsZoomEnabled;
			oldSetting.BehaviorTypeId = settingModel.BehaviorTypeId;
			await _imageSettingRepository.UpdateAsync(oldSetting);
			await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.PictureSetting360Prefix, oldSetting.Id);
		}
		else
		{
			ProductImageSetting360 entity = settingModel.ToEntity<ProductImageSetting360>();
			await _imageSettingRepository.InsertAsync(entity);
		}
	}

	public virtual async Task DeleteImageSettingAsync(ProductImageSetting360 setting)
	{
		await _imageSettingRepository.DeleteAsync(setting);
		await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.PictureSetting360Prefix, setting.Id);
	}

	public virtual async Task<ProductImageSetting360> GetImageSettingByProductIdAsync(int productId)
	{
		return await _imageSettingRepository.Table.Where((ProductImageSetting360 st) => st.ProductId == productId).FirstOrDefaultAsync();
	}
}
