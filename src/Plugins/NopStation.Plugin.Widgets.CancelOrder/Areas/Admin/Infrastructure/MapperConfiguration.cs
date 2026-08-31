using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.CancelOrder.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.CancelOrder.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 0;

	public MapperConfiguration()
	{
		CreateMap<CancelOrderSettings, ConfigurationModel>().ForMember((ConfigurationModel dest) => dest.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<CancelOrderSettings, ConfigurationModel, int> mo)
		{
			mo.Ignore();
		}).ForMember((ConfigurationModel dest) => dest.AvailableOrderStatuses, delegate(IMemberConfigurationExpression<CancelOrderSettings, ConfigurationModel, IList<SelectListItem>> mo)
		{
			mo.Ignore();
		}).ForMember((ConfigurationModel dest) => dest.AvailablePaymentStatuses, delegate(IMemberConfigurationExpression<CancelOrderSettings, ConfigurationModel, IList<SelectListItem>> mo)
		{
			mo.Ignore();
		})
			.ForMember((ConfigurationModel dest) => dest.AvailableShippingStatuses, delegate(IMemberConfigurationExpression<CancelOrderSettings, ConfigurationModel, IList<SelectListItem>> mo)
			{
				mo.Ignore();
			})
			.ForMember((ConfigurationModel dest) => dest.WidgetZone_OverrideForStore, delegate(IMemberConfigurationExpression<CancelOrderSettings, ConfigurationModel, bool> mo)
			{
				mo.Ignore();
			})
			.ForMember((ConfigurationModel dest) => dest.CancellableOrderStatuses_OverrideForStore, delegate(IMemberConfigurationExpression<CancelOrderSettings, ConfigurationModel, bool> mo)
			{
				mo.Ignore();
			})
			.ForMember((ConfigurationModel dest) => dest.CancellablePaymentStatuses_OverrideForStore, delegate(IMemberConfigurationExpression<CancelOrderSettings, ConfigurationModel, bool> mo)
			{
				mo.Ignore();
			})
			.ForMember((ConfigurationModel dest) => dest.CancellableShippingStatuses_OverrideForStore, delegate(IMemberConfigurationExpression<CancelOrderSettings, ConfigurationModel, bool> mo)
			{
				mo.Ignore();
			});
		CreateMap<ConfigurationModel, CancelOrderSettings>();
	}
}
