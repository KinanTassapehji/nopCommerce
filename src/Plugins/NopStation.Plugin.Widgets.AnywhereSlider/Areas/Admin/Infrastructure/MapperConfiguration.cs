using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<SliderSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.EnableSlider_OverrideForStore, delegate(IMemberConfigurationExpression<SliderSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<SliderSettings, ConfigurationModel, Dictionary<string, string>> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<SliderSettings, ConfigurationModel, int> options)
		{
			options.Ignore();
		});
		CreateMap<ConfigurationModel, SliderSettings>();
		CreateMap<Slider, SliderModel>().ForMember((SliderModel model) => model.AvailableStores, delegate(IMemberConfigurationExpression<Slider, SliderModel, IList<SelectListItem>> options)
		{
			options.Ignore();
		}).ForMember((SliderModel model) => model.AvailableWidgetZones, delegate(IMemberConfigurationExpression<Slider, SliderModel, IList<SelectListItem>> options)
		{
			options.Ignore();
		}).ForMember((SliderModel model) => model.WidgetZoneStr, delegate(IMemberConfigurationExpression<Slider, SliderModel, string> options)
		{
			options.Ignore();
		})
			.ForMember((SliderModel model) => model.CreatedOn, delegate(IMemberConfigurationExpression<Slider, SliderModel, DateTime> options)
			{
				options.Ignore();
			})
			.ForMember((SliderModel model) => model.UpdatedOn, delegate(IMemberConfigurationExpression<Slider, SliderModel, DateTime> options)
			{
				options.Ignore();
			})
			.ForMember((SliderModel model) => model.SliderItemSearchModel, delegate(IMemberConfigurationExpression<Slider, SliderModel, SliderItemSearchModel> options)
			{
				options.Ignore();
			})
			.ForMember((SliderModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<Slider, SliderModel, Dictionary<string, string>> options)
			{
				options.Ignore();
			})
			.ForMember((SliderModel model) => model.SelectedStoreIds, delegate(IMemberConfigurationExpression<Slider, SliderModel, IList<int>> options)
			{
				options.Ignore();
			});
		CreateMap<SliderModel, Slider>().ForMember((Slider entity) => entity.CreatedOnUtc, delegate(IMemberConfigurationExpression<SliderModel, Slider, DateTime> options)
		{
			options.Ignore();
		}).ForMember((Slider entity) => entity.UpdatedOnUtc, delegate(IMemberConfigurationExpression<SliderModel, Slider, DateTime> options)
		{
			options.Ignore();
		});
		CreateMap<SliderItem, SliderItemModel>().ForMember((SliderItemModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<SliderItem, SliderItemModel, Dictionary<string, string>> options)
		{
			options.Ignore();
		}).ForMember((SliderItemModel model) => model.FullPictureUrl, delegate(IMemberConfigurationExpression<SliderItem, SliderItemModel, string> options)
		{
			options.Ignore();
		}).ForMember((SliderItemModel model) => model.PictureUrl, delegate(IMemberConfigurationExpression<SliderItem, SliderItemModel, string> options)
		{
			options.Ignore();
		})
			.ForMember((SliderItemModel model) => model.MobileFullPictureUrl, delegate(IMemberConfigurationExpression<SliderItem, SliderItemModel, string> options)
			{
				options.Ignore();
			})
			.ForMember((SliderItemModel model) => model.MobilePictureUrl, delegate(IMemberConfigurationExpression<SliderItem, SliderItemModel, string> options)
			{
				options.Ignore();
			})
			.ForMember((SliderItemModel model) => model.SliderItemTitle, delegate(IMemberConfigurationExpression<SliderItem, SliderItemModel, string> options)
			{
				options.MapFrom((SliderItem s) => s.Title);
			});
		CreateMap<SliderItemModel, SliderItem>().ForMember((SliderItem model) => model.Title, delegate(IMemberConfigurationExpression<SliderItemModel, SliderItem, string> options)
		{
			options.MapFrom((SliderItemModel s) => s.SliderItemTitle);
		});
	}
}
