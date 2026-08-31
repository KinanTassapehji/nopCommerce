using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;
using NopStation.Plugin.Misc.Core.Domains.SMS;
using NopStation.Plugin.Misc.Core.Services.SMS;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<SmsTemplate, SmsTemplateModel>().ForMember((SmsTemplateModel model) => model.AllowedTokens, delegate(IMemberConfigurationExpression<SmsTemplate, SmsTemplateModel, string> options)
		{
			options.Ignore();
		});
		CreateMap<SmsTemplateModel, SmsTemplate>().ForMember((SmsTemplate entity) => entity.Name, delegate(IMemberConfigurationExpression<SmsTemplateModel, SmsTemplate, string> options)
		{
			options.Ignore();
		});
		CreateMap<QueuedSms, QueuedSmsModel>().ForMember((QueuedSmsModel model) => model.CreatedOn, delegate(IMemberConfigurationExpression<QueuedSms, QueuedSmsModel, DateTime> options)
		{
			options.Ignore();
		}).ForMember((QueuedSmsModel model) => model.SentOn, delegate(IMemberConfigurationExpression<QueuedSms, QueuedSmsModel, DateTime?> options)
		{
			options.Ignore();
		});
		CreateMap<QueuedSmsModel, QueuedSms>().ForMember((QueuedSms entity) => entity.CreatedOnUtc, delegate(IMemberConfigurationExpression<QueuedSmsModel, QueuedSms, DateTime> options)
		{
			options.Ignore();
		}).ForMember((QueuedSms entity) => entity.SentOnUtc, delegate(IMemberConfigurationExpression<QueuedSmsModel, QueuedSms, DateTime?> options)
		{
			options.Ignore();
		});
		CreateMap<ISmsPlugin, SmsProviderModel>().ForMember((SmsProviderModel model) => model.FriendlyName, delegate(IMemberConfigurationExpression<ISmsPlugin, SmsProviderModel, string> options)
		{
			options.MapFrom((ISmsPlugin plugin) => plugin.PluginDescriptor.FriendlyName);
		}).ForMember((SmsProviderModel model) => model.SystemName, delegate(IMemberConfigurationExpression<ISmsPlugin, SmsProviderModel, string> options)
		{
			options.MapFrom((ISmsPlugin plugin) => plugin.PluginDescriptor.SystemName);
		}).ForMember((SmsProviderModel model) => model.DisplayOrder, delegate(IMemberConfigurationExpression<ISmsPlugin, SmsProviderModel, int> options)
		{
			options.MapFrom((ISmsPlugin plugin) => plugin.PluginDescriptor.DisplayOrder);
		});
		CreateMap<CoreLocaleResourceModel, LocaleStringResource>().ForMember((LocaleStringResource entity) => entity.LanguageId, delegate(IMemberConfigurationExpression<CoreLocaleResourceModel, LocaleStringResource, int> options)
		{
			options.Ignore();
		});
		CreateMap<NopStationCoreSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.AvailableCustomerRoles, delegate(IMemberConfigurationExpression<NopStationCoreSettings, ConfigurationModel, IList<SelectListItem>> options)
		{
			options.Ignore();
		});
		CreateMap<ConfigurationModel, NopStationCoreSettings>();
		CreateMap<SmsSettings, SmsSettingsModel>();
		CreateMap<SmsSettingsModel, SmsSettings>();
	}
}
