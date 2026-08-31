using System.Collections.Generic;
using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.QuickView.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.QuickView.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<QuickViewSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.ShowRelatedProducts_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.ShowAlsoPurchasedProducts_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.ShowAddToWishlistButton_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		})
			.ForMember((ConfigurationModel model) => model.ShowAvailability_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowCompareProductsButton_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowDeliveryInfo_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowFullDescription_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowProductEmailAFriendButton_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowProductManufacturers_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowProductReviewOverview_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowProductTags_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowRelatedProducts_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowShortDescription_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.EnablePictureZoom_OverrideForStore, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, Dictionary<string, string>> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.PictureZoomPluginInstalled, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<QuickViewSettings, ConfigurationModel, int> options)
			{
				options.Ignore();
			});
		CreateMap<ConfigurationModel, QuickViewSettings>();
	}
}
