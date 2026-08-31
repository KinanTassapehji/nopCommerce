using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;
using NopStation.Plugin.Widgets.AnywhereSlider.Infrastructure.Cache;
using NopStation.Plugin.Widgets.AnywhereSlider.Models;
using NopStation.Plugin.Widgets.AnywhereSlider.Services;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Factories;

public class SliderModelFactory : ISliderModelFactory
{
	private readonly IPictureService _pictureService;

	private readonly ISliderService _sliderService;

	private readonly ILocalizationService _localizationService;

	private readonly IWorkContext _workContext;

	private readonly INopStationContext _nopStationContext;

	private readonly IStoreContext _storeContext;

	private readonly IStaticCacheManager _cacheManager;

	public SliderModelFactory(IPictureService pictureService, ISliderService sliderService, ILocalizationService localizationService, IWorkContext workContext, INopStationContext nopStationContext, IStoreContext storeContext, IStaticCacheManager cacheManager)
	{
		_pictureService = pictureService;
		_sliderService = sliderService;
		_localizationService = localizationService;
		_workContext = workContext;
		_nopStationContext = nopStationContext;
		_storeContext = storeContext;
		_cacheManager = cacheManager;
	}

	protected async Task<string> GetSliderBackgroundImage(Slider slider)
	{
		CacheKey key = _cacheManager.PrepareKeyForDefaultCache(SliderCacheDefaults.SliderBackgrounPictureKey, slider, _storeContext.GetCurrentStoreAsync());
		return await _cacheManager.GetAsync(key, async () => await _pictureService.GetPictureUrlAsync(slider.BackgroundPictureId));
	}

	public async Task<SliderListModel> PrepareSliderListModelAsync(int widgetZoneId)
	{
		List<int> widgetZoneIds = new List<int> { widgetZoneId };
		IPagedList<Slider> pagedList = await _sliderService.GetAllSlidersAsync(widgetZoneIds, _storeContext.GetCurrentStore().Id, true);
		SliderListModel model = new SliderListModel();
		foreach (Slider slider in pagedList)
		{
			List<SliderListModel.SliderOverviewModel> sliders = model.Sliders;
			SliderListModel.SliderOverviewModel sliderOverviewModel = new SliderListModel.SliderOverviewModel
			{
				ShowBackgroundPicture = slider.ShowBackgroundPicture
			};
			SliderListModel.SliderOverviewModel sliderOverviewModel2 = sliderOverviewModel;
			string backgroundPictureUrl = ((!slider.ShowBackgroundPicture) ? "" : (await GetSliderBackgroundImage(slider)));
			sliderOverviewModel2.BackgroundPictureUrl = backgroundPictureUrl;
			sliderOverviewModel.Id = slider.Id;
			sliders.Add(sliderOverviewModel);
		}
		return model;
	}

	public async Task<SliderModel> PrepareSliderModelAsync(Slider slider)
	{
		IStaticCacheManager cacheManager = _cacheManager;
		CacheKey sliderModelKey = SliderCacheDefaults.SliderModelKey;
		object obj = slider;
		object obj2 = _nopStationContext.MobileDevice;
		object obj3 = await _workContext.GetWorkingLanguageAsync();
		object currentStore = _storeContext.GetCurrentStore();
		Customer customer = await _workContext.GetCurrentCustomerAsync();
		CacheKey key = cacheManager.PrepareKeyForDefaultCache(sliderModelKey, obj, obj2, obj3, currentStore, customer);
		return await _cacheManager.GetAsync(key, async delegate
		{
			SliderModel sliderModel = new SliderModel
			{
				Id = slider.Id
			};
			SliderModel sliderModel2 = sliderModel;
			sliderModel2.Name = await _localizationService.GetLocalizedAsync(slider, (Slider x) => x.Name);
			sliderModel.WidgetZoneId = slider.WidgetZoneId;
			sliderModel.Nav = slider.Nav;
			sliderModel.AutoPlayHoverPause = slider.AutoPlayHoverPause;
			sliderModel.StartPosition = slider.StartPosition;
			sliderModel.LazyLoad = slider.LazyLoad;
			sliderModel.LazyLoadEager = slider.LazyLoadEager;
			sliderModel.Video = slider.Video;
			sliderModel.AnimateOut = slider.AnimateOut;
			sliderModel.AnimateIn = slider.AnimateIn;
			sliderModel.Loop = slider.Loop;
			sliderModel.Margin = slider.Margin;
			sliderModel.AutoPlay = slider.AutoPlay;
			sliderModel.AutoPlayTimeout = slider.AutoPlayTimeout;
			SliderModel sliderModel3 = sliderModel;
			sliderModel3.Rtl = (await _workContext.GetWorkingLanguageAsync()).Rtl;
			SliderModel sliderModel4 = sliderModel;
			if (slider.WidgetZoneId != 5)
			{
				sliderModel = sliderModel4;
				sliderModel.BackGroundPictureUrl = await _pictureService.GetPictureUrlAsync(slider.BackgroundPictureId);
			}
			foreach (SliderItem si in await _sliderService.GetSliderItemsBySliderIdAsync(slider.Id))
			{
				IList<SliderModel.SliderItemModel> items = sliderModel4.Items;
				SliderModel.SliderItemModel sliderItemModel = new SliderModel.SliderItemModel
				{
					Id = si.Id
				};
				SliderModel.SliderItemModel sliderItemModel2 = sliderItemModel;
				sliderItemModel2.Title = await _localizationService.GetLocalizedAsync(si, (SliderItem x) => x.Title);
				SliderModel.SliderItemModel sliderItemModel3 = sliderItemModel;
				sliderItemModel3.Link = await _localizationService.GetLocalizedAsync(si, (SliderItem x) => x.Link);
				SliderModel.SliderItemModel sliderItemModel4 = sliderItemModel;
				sliderItemModel4.ShopNowLink = await _localizationService.GetLocalizedAsync(si, (SliderItem x) => x.ShopNowLink);
				SliderModel.SliderItemModel sliderItemModel5 = sliderItemModel;
				sliderItemModel5.PictureUrl = await _pictureService.GetPictureUrlAsync(_nopStationContext.MobileDevice ? si.MobilePictureId : si.PictureId);
				SliderModel.SliderItemModel sliderItemModel6 = sliderItemModel;
				sliderItemModel6.ImageAltText = await _localizationService.GetLocalizedAsync(si, (SliderItem x) => x.ImageAltText);
				SliderModel.SliderItemModel sliderItemModel7 = sliderItemModel;
				sliderItemModel7.ShortDescription = await _localizationService.GetLocalizedAsync(si, (SliderItem x) => x.ShortDescription);
				items.Add(sliderItemModel);
			}
			return sliderModel4;
		});
	}
}
