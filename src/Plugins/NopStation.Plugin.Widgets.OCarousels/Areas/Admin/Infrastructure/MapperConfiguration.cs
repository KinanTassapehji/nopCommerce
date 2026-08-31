using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.OCarousels.Domains;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<OCarouselSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.EnableOCarousel_OverrideForStore, delegate(IMemberConfigurationExpression<OCarouselSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<OCarouselSettings, ConfigurationModel, Dictionary<string, string>> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<OCarouselSettings, ConfigurationModel, int> options)
		{
			options.Ignore();
		});
		CreateMap<ConfigurationModel, OCarouselSettings>();
		CreateMap<OCarousel, OCarouselModel>().ForMember((OCarouselModel model) => model.AvailableStores, delegate(IMemberConfigurationExpression<OCarousel, OCarouselModel, IList<SelectListItem>> options)
		{
			options.Ignore();
		}).ForMember((OCarouselModel model) => model.AvailableWidgetZones, delegate(IMemberConfigurationExpression<OCarousel, OCarouselModel, IList<SelectListItem>> options)
		{
			options.Ignore();
		}).ForMember((OCarouselModel model) => model.AvailableDataSources, delegate(IMemberConfigurationExpression<OCarousel, OCarouselModel, IList<SelectListItem>> options)
		{
			options.Ignore();
		})
			.ForMember((OCarouselModel model) => model.DataSourceTypeStr, delegate(IMemberConfigurationExpression<OCarousel, OCarouselModel, string> options)
			{
				options.Ignore();
			})
			.ForMember((OCarouselModel model) => model.WidgetZoneStr, delegate(IMemberConfigurationExpression<OCarousel, OCarouselModel, string> options)
			{
				options.Ignore();
			})
			.ForMember((OCarouselModel model) => model.CreatedOn, delegate(IMemberConfigurationExpression<OCarousel, OCarouselModel, DateTime> options)
			{
				options.Ignore();
			})
			.ForMember((OCarouselModel model) => model.UpdatedOn, delegate(IMemberConfigurationExpression<OCarousel, OCarouselModel, DateTime> options)
			{
				options.Ignore();
			})
			.ForMember((OCarouselModel model) => model.OCarouselItemSearchModel, delegate(IMemberConfigurationExpression<OCarousel, OCarouselModel, OCarouselItemSearchModel> options)
			{
				options.Ignore();
			})
			.ForMember((OCarouselModel model) => model.OCarouselItemSearchModel, delegate(IMemberConfigurationExpression<OCarousel, OCarouselModel, OCarouselItemSearchModel> options)
			{
				options.Ignore();
			})
			.ForMember((OCarouselModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<OCarousel, OCarouselModel, Dictionary<string, string>> options)
			{
				options.Ignore();
			})
			.ForMember((OCarouselModel model) => model.SelectedStoreIds, delegate(IMemberConfigurationExpression<OCarousel, OCarouselModel, IList<int>> options)
			{
				options.Ignore();
			});
		CreateMap<OCarouselModel, OCarousel>().ForMember((OCarousel entity) => entity.CreatedOnUtc, delegate(IMemberConfigurationExpression<OCarouselModel, OCarousel, DateTime> options)
		{
			options.Ignore();
		}).ForMember((OCarousel entity) => entity.UpdatedOnUtc, delegate(IMemberConfigurationExpression<OCarouselModel, OCarousel, DateTime> options)
		{
			options.Ignore();
		});
		CreateMap<OCarouselItem, OCarouselItemModel>().ForMember((OCarouselItemModel model) => model.ProductName, delegate(IMemberConfigurationExpression<OCarouselItem, OCarouselItemModel, string> options)
		{
			options.Ignore();
		}).ForMember((OCarouselItemModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<OCarouselItem, OCarouselItemModel, Dictionary<string, string>> options)
		{
			options.Ignore();
		}).ForMember((OCarouselItemModel model) => model.PictureUrl, delegate(IMemberConfigurationExpression<OCarouselItem, OCarouselItemModel, string> options)
		{
			options.Ignore();
		});
		CreateMap<OCarouselItemModel, OCarouselItem>();
	}
}
