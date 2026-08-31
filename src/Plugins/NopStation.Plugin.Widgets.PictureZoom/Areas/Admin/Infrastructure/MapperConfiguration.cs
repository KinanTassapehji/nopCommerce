using System.Collections.Generic;
using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.PictureZoom.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.PictureZoom.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<PictureZoomSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.AdjustX_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.AdjustY_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.FullSizeImage_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		})
			.ForMember((ConfigurationModel model) => model.ImageSize_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.LensOpacity_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.LtrPositionTypeId_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.RtlPositionTypeId_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowTitle_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.SmoothMove_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.SoftFocus_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.TintOpacity_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.Tint_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.TitleOpacity_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ZoomHeight_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ZoomWidth_OverrideForStore, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, Dictionary<string, string>> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<PictureZoomSettings, ConfigurationModel, int> options)
			{
				options.Ignore();
			});
		CreateMap<ConfigurationModel, PictureZoomSettings>();
	}
}
