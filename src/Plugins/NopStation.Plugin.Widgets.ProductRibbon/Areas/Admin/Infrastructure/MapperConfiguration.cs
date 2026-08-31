using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.ProductRibbon.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProductRibbon.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<ProductRibbonSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.EnableBestSellerRibbon_OverrideForStore, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.EnableDiscountRibbon_OverrideForStore, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.EnableNewRibbon_OverrideForStore, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		})
			.ForMember((ConfigurationModel model) => model.ProductDetailsPageWidgetZone_OverrideForStore, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.BestSellOrderStatusIds_OverrideForStore, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.BestSellPaymentStatusIds_OverrideForStore, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.BestSellShippingStatusIds_OverrideForStore, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.BestSellStoreWise_OverrideForStore, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.MinimumAmountSold_OverrideForStore, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.MinimumQuantitySold_OverrideForStore, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.SoldInDays_OverrideForStore, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.AvailableOrderStatuses, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, IList<SelectListItem>> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.AvailablePaymentStatuses, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, IList<SelectListItem>> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.AvailableShippingStatuses, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, IList<SelectListItem>> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, Dictionary<string, string>> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.CurrencyCode, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, string> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<ProductRibbonSettings, ConfigurationModel, int> options)
			{
				options.Ignore();
			});
		CreateMap<ConfigurationModel, ProductRibbonSettings>();
	}
}
