using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;
using NopStation.Plugin.Widgets.AnywhereSlider.Helpers;
using NopStation.Plugin.Widgets.AnywhereSlider.Services;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Factories;

public class SliderModelFactory : ISliderModelFactory
{
	private readonly IStoreContext _storeContext;

	private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

	private readonly ILocalizedModelFactory _localizedModelFactory;

	private readonly IBaseAdminModelFactory _baseAdminModelFactory;

	private readonly ILocalizationService _localizationService;

	private readonly IPictureService _pictureService;

	private readonly ISettingService _settingService;

	private readonly IDateTimeHelper _dateTimeHelper;

	private readonly ISliderService _sliderService;

	public SliderModelFactory(IStoreContext storeContext, IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory, ILocalizedModelFactory localizedModelFactory, IBaseAdminModelFactory baseAdminModelFactory, ILocalizationService localizationService, IPictureService pictureService, ISettingService settingService, IDateTimeHelper dateTimeHelper, ISliderService sliderService)
	{
		_storeContext = storeContext;
		_storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
		_localizedModelFactory = localizedModelFactory;
		_baseAdminModelFactory = baseAdminModelFactory;
		_localizationService = localizationService;
		_pictureService = pictureService;
		_settingService = settingService;
		_dateTimeHelper = dateTimeHelper;
		_sliderService = sliderService;
	}

	protected async Task PrepareCustomWidgetZonesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
	{
		ArgumentNullException.ThrowIfNull(items, "items");
		foreach (SelectListItem customWidgetZoneSelect in SliderHelper.GetCustomWidgetZoneSelectList())
		{
			items.Add(customWidgetZoneSelect);
		}
		if (withSpecialDefaultItem)
		{
			SelectListItem selectListItem = new SelectListItem();
			SelectListItem selectListItem2 = selectListItem;
			selectListItem2.Text = await _localizationService.GetResourceAsync("Admin.Common.All");
			selectListItem.Value = "0";
			items.Insert(0, selectListItem);
		}
	}

	protected async Task PrepareActiveOptionsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
	{
		ArgumentNullException.ThrowIfNull(items, "items");
		IList<SelectListItem> list = items;
		SelectListItem selectListItem = new SelectListItem();
		SelectListItem selectListItem2 = selectListItem;
		selectListItem2.Text = await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Sliders.List.SearchActive.Active");
		selectListItem.Value = "1";
		list.Add(selectListItem);
		list = items;
		selectListItem2 = new SelectListItem();
		selectListItem = selectListItem2;
		selectListItem.Text = await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Sliders.List.SearchActive.Inactive");
		selectListItem2.Value = "2";
		list.Add(selectListItem2);
		if (withSpecialDefaultItem)
		{
			list = items;
			selectListItem = new SelectListItem();
			selectListItem2 = selectListItem;
			selectListItem2.Text = await _localizationService.GetResourceAsync("Admin.Common.All");
			selectListItem.Value = "0";
			list.Insert(0, selectListItem);
		}
	}

	public virtual async Task<SliderSearchModel> PrepareSliderSearchModelAsync(SliderSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		await PrepareCustomWidgetZonesAsync(searchModel.AvailableWidgetZones);
		await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions);
		await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);
		searchModel.SetGridPageSize();
		return searchModel;
	}

	public virtual async Task<SliderListModel> PrepareSliderListModelAsync(SliderSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		List<int> widgetZoneIds = ((searchModel.SearchWidgetZones?.Contains(0) ?? true) ? null : searchModel.SearchWidgetZones.ToList());
		bool? active = searchModel.SearchActiveId switch
		{
			1 => true, 
			2 => false, 
			_ => null, 
		};
		IPagedList<Slider> sliders = await _sliderService.GetAllSlidersAsync(widgetZoneIds, searchModel.SearchStoreId, active, searchModel.Page - 1, searchModel.PageSize);
		return await new SliderListModel().PrepareToGridAsync(searchModel, sliders, () => sliders.SelectAwait<Slider, SliderModel>(async (Slider slider) => await PrepareSliderModelAsync(null, slider, excludeProperties: true)));
	}

	public async Task<SliderModel> PrepareSliderModelAsync(SliderModel model, Slider slider, bool excludeProperties = false)
	{
		Func<SliderLocalizedModel, int, Task> localizedModelConfiguration = null;
		if (slider != null && model == null)
		{
			model = slider.ToModel<SliderModel>();
			model.WidgetZoneStr = SliderHelper.GetCustomWidgetZone(slider.WidgetZoneId);
			SliderModel sliderModel = model;
			sliderModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(slider.CreatedOnUtc, DateTimeKind.Utc);
			sliderModel = model;
			sliderModel.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(slider.UpdatedOnUtc, DateTimeKind.Utc);
			if (!excludeProperties)
			{
				localizedModelConfiguration = async delegate(SliderLocalizedModel locale, int languageId)
				{
					locale.Name = await _localizationService.GetLocalizedAsync(slider, (Slider entity) => entity.Name, languageId, returnDefaultValue: false, ensureTwoPublishedLanguages: false);
				};
			}
		}
		if (!excludeProperties)
		{
			SliderModel sliderModel = model;
			sliderModel.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
			model.AvailableWidgetZones = SliderHelper.GetCustomWidgetZoneSelectList();
			model.AvailableAnimationTypes = SliderHelper.GetSliderAnimationTypesSelectList();
			await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, slider, excludeProperties);
		}
		return model;
	}

	public async Task<SliderItemListModel> PrepareSliderItemListModelAsync(SliderItemSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		IPagedList<SliderItem> sliderItems = await _sliderService.GetSliderItemsBySliderIdAsync(searchModel.SliderId, searchModel.Page - 1, searchModel.PageSize);
		return await new SliderItemListModel().PrepareToGridAsync(searchModel, sliderItems, () => sliderItems.SelectAwait<SliderItem, SliderItemModel>(async delegate(SliderItem sliderItem)
		{
			Slider slider = await _sliderService.GetSliderByIdAsync(sliderItem.SliderId);
			return await PrepareSliderItemModelAsync(null, slider, sliderItem);
		}));
	}

	public async Task<SliderItemModel> PrepareSliderItemModelAsync(SliderItemModel model, Slider slider, SliderItem sliderItem, bool excludeProperties = false)
	{
		Func<SliderItemLocalizedModel, int, Task> localizedModelConfiguration = null;
		if (sliderItem != null)
		{
			if (model == null)
			{
				model = sliderItem.ToModel<SliderItemModel>();
				SliderItemModel sliderItemModel = model;
				sliderItemModel.PictureUrl = await _pictureService.GetPictureUrlAsync(sliderItem.PictureId, 200);
				sliderItemModel = model;
				sliderItemModel.FullPictureUrl = await _pictureService.GetPictureUrlAsync(sliderItem.PictureId);
				sliderItemModel = model;
				sliderItemModel.MobilePictureUrl = await _pictureService.GetPictureUrlAsync(sliderItem.MobilePictureId, 200);
				sliderItemModel = model;
				sliderItemModel.MobileFullPictureUrl = await _pictureService.GetPictureUrlAsync(sliderItem.MobilePictureId);
				model.SliderItemTitle = sliderItem.Title;
			}
			if (!excludeProperties)
			{
				localizedModelConfiguration = async delegate(SliderItemLocalizedModel locale, int languageId)
				{
					SliderItemLocalizedModel sliderItemLocalizedModel = locale;
					sliderItemLocalizedModel.SliderItemTitle = await _localizationService.GetLocalizedAsync(sliderItem, (SliderItem entity) => entity.Title, languageId);
					sliderItemLocalizedModel = locale;
					sliderItemLocalizedModel.ShortDescription = await _localizationService.GetLocalizedAsync(sliderItem, (SliderItem entity) => entity.ShortDescription, languageId);
					sliderItemLocalizedModel = locale;
					sliderItemLocalizedModel.ImageAltText = await _localizationService.GetLocalizedAsync(sliderItem, (SliderItem entity) => entity.ImageAltText, languageId);
					sliderItemLocalizedModel = locale;
					sliderItemLocalizedModel.Link = await _localizationService.GetLocalizedAsync(sliderItem, (SliderItem entity) => entity.Link, languageId);
					sliderItemLocalizedModel = locale;
					sliderItemLocalizedModel.ShopNowLink = await _localizationService.GetLocalizedAsync(sliderItem, (SliderItem entity) => entity.ShopNowLink, languageId);
				};
			}
		}
		if (!excludeProperties)
		{
			SliderItemModel sliderItemModel = model;
			sliderItemModel.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
		}
		model.SliderId = slider.Id;
		return model;
	}
}
