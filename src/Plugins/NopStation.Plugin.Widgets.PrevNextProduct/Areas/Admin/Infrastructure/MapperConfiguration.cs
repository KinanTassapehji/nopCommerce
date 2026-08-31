using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.PrevNextProduct.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<PrevNextProductSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<PrevNextProductSettings, ConfigurationModel, int> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.AvailableNavigationTypes, delegate(IMemberConfigurationExpression<PrevNextProductSettings, ConfigurationModel, IList<SelectListItem>> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.EnableLoop_OverrideForStore, delegate(IMemberConfigurationExpression<PrevNextProductSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		})
			.ForMember((ConfigurationModel model) => model.WidgetZone_OverrideForStore, delegate(IMemberConfigurationExpression<PrevNextProductSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.NavigateBasedOnId_OverrideForStore, delegate(IMemberConfigurationExpression<PrevNextProductSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ProductNameMaxLength_OverrideForStore, delegate(IMemberConfigurationExpression<PrevNextProductSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			});
		CreateMap<ConfigurationModel, PrevNextProductSettings>();
	}
}
