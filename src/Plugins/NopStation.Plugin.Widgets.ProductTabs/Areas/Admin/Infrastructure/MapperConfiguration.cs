using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductTabs.Domains;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<ProductTabSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.EnableProductTab_OverrideForStore, delegate(IMemberConfigurationExpression<ProductTabSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<ProductTabSettings, ConfigurationModel, Dictionary<string, string>> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<ProductTabSettings, ConfigurationModel, int> options)
		{
			options.Ignore();
		});
		CreateMap<ConfigurationModel, ProductTabSettings>();
		CreateMap<ProductTab, ProductTabModel>().ForMember((ProductTabModel model) => model.WidgetZoneStr, delegate(IMemberConfigurationExpression<ProductTab, ProductTabModel, string> options)
		{
			options.Ignore();
		}).ForMember((ProductTabModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<ProductTab, ProductTabModel, Dictionary<string, string>> options)
		{
			options.Ignore();
		}).ForMember((ProductTabModel model) => model.ProductTabItemSearchModel, delegate(IMemberConfigurationExpression<ProductTab, ProductTabModel, ProductTabItemSearchModel> options)
		{
			options.Ignore();
		})
			.ForMember((ProductTabModel model) => model.AvailableWidgetZones, delegate(IMemberConfigurationExpression<ProductTab, ProductTabModel, IList<SelectListItem>> options)
			{
				options.Ignore();
			})
			.ForMember((ProductTabModel model) => model.Locales, delegate(IMemberConfigurationExpression<ProductTab, ProductTabModel, IList<ProductTabLocalizedModel>> options)
			{
				options.Ignore();
			})
			.ForMember((ProductTabModel model) => model.SelectedStoreIds, delegate(IMemberConfigurationExpression<ProductTab, ProductTabModel, IList<int>> options)
			{
				options.Ignore();
			})
			.ForMember((ProductTabModel model) => model.AvailableStores, delegate(IMemberConfigurationExpression<ProductTab, ProductTabModel, IList<SelectListItem>> options)
			{
				options.Ignore();
			})
			.ForMember((ProductTabModel model) => model.CreatedOn, delegate(IMemberConfigurationExpression<ProductTab, ProductTabModel, DateTime> options)
			{
				options.Ignore();
			})
			.ForMember((ProductTabModel model) => model.UpdatedOn, delegate(IMemberConfigurationExpression<ProductTab, ProductTabModel, DateTime> options)
			{
				options.Ignore();
			});
		CreateMap<ProductTabModel, ProductTab>().ForMember((ProductTab entity) => entity.CreatedOnUtc, delegate(IMemberConfigurationExpression<ProductTabModel, ProductTab, DateTime> options)
		{
			options.Ignore();
		}).ForMember((ProductTab entity) => entity.UpdatedOnUtc, delegate(IMemberConfigurationExpression<ProductTabModel, ProductTab, DateTime> options)
		{
			options.Ignore();
		}).ForMember((ProductTab entity) => entity.LimitedToStores, delegate(IMemberConfigurationExpression<ProductTabModel, ProductTab, bool> options)
		{
			options.Ignore();
		});
		CreateMap<ProductTabItem, ProductTabItemModel>().ForMember((ProductTabItemModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<ProductTabItem, ProductTabItemModel, Dictionary<string, string>> options)
		{
			options.Ignore();
		}).ForMember((ProductTabItemModel model) => model.ProductSearchModel, delegate(IMemberConfigurationExpression<ProductTabItem, ProductTabItemModel, ProductTabItemProductSearchModel> options)
		{
			options.Ignore();
		}).ForMember((ProductTabItemModel model) => model.Locales, delegate(IMemberConfigurationExpression<ProductTabItem, ProductTabItemModel, IList<ProductTabItemLocalizedModel>> options)
		{
			options.Ignore();
		});
		CreateMap<ProductTabItemModel, ProductTabItem>();
		CreateMap<ProductTabItemProduct, ProductTabItemProductModel>().ForMember((ProductTabItemProductModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<ProductTabItemProduct, ProductTabItemProductModel, Dictionary<string, string>> options)
		{
			options.Ignore();
		}).ForMember((ProductTabItemProductModel model) => model.ProductName, delegate(IMemberConfigurationExpression<ProductTabItemProduct, ProductTabItemProductModel, string> options)
		{
			options.Ignore();
		});
		CreateMap<ProductTabItemProductModel, ProductTabItemProduct>();
	}
}
