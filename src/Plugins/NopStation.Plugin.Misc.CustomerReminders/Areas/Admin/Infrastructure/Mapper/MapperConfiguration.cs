using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Settings;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Infrastructure.Mapper;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<Reminder, ReminderModel>().ForMember((ReminderModel model) => model.AvailableStores, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, IList<SelectListItem>> options)
		{
			options.Ignore();
		}).ForMember((ReminderModel model) => model.AvailableVendors, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, IList<SelectListItem>> options)
		{
			options.Ignore();
		}).ForMember((ReminderModel model) => model.AvailableIntervalBetweenMessagesTypes, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, IList<SelectListItem>> options)
		{
			options.Ignore();
		})
			.ForMember((ReminderModel model) => model.AvailableDateGreaterThanIntervalTypes, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, IList<SelectListItem>> options)
			{
				options.Ignore();
			})
			.ForMember((ReminderModel model) => model.AvailableDateLowerThanIntervalTypes, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, IList<SelectListItem>> options)
			{
				options.Ignore();
			})
			.ForMember((ReminderModel model) => model.AvailableReminderRules, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, IList<SelectListItem>> options)
			{
				options.Ignore();
			})
			.ForMember((ReminderModel model) => model.AvailableTokensFromRule, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, string> options)
			{
				options.Ignore();
			})
			.ForMember((ReminderModel model) => model.AvailableEmailAccounts, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, IList<SelectListItem>> options)
			{
				options.Ignore();
			})
			.ForMember((ReminderModel model) => model.MessageTemplateName, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, string> options)
			{
				options.Ignore();
			})
			.ForMember((ReminderModel model) => model.MessageTemplateBcc, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, string> options)
			{
				options.Ignore();
			})
			.ForMember((ReminderModel model) => model.MessageTemplateSubject, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, string> options)
			{
				options.Ignore();
			})
			.ForMember((ReminderModel model) => model.MessageTemplateBody, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, string> options)
			{
				options.Ignore();
			})
			.ForMember((ReminderModel model) => model.EmailAccountId, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, int> options)
			{
				options.Ignore();
			})
			.ForMember((ReminderModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<Reminder, ReminderModel, Dictionary<string, string>> options)
			{
				options.Ignore();
			});
		CreateMap<ReminderModel, Reminder>().ForMember((Reminder entity) => entity.ExecutedOnUtc, delegate(IMemberConfigurationExpression<ReminderModel, Reminder, DateTime> options)
		{
			options.Ignore();
		}).ForMember((Reminder entity) => entity.MessageTemplateId, delegate(IMemberConfigurationExpression<ReminderModel, Reminder, int> options)
		{
			options.Ignore();
		});
		CreateMap<ReminderRule, ReminderRuleModel>().ForMember((ReminderRuleModel model) => model.AvailableTokens, delegate(IMemberConfigurationExpression<ReminderRule, ReminderRuleModel, IList<SelectListItem>> options)
		{
			options.Ignore();
		}).ForMember((ReminderRuleModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<ReminderRule, ReminderRuleModel, Dictionary<string, string>> options)
		{
			options.Ignore();
		});
		CreateMap<ReminderRuleModel, ReminderRule>().ForMember((ReminderRule entity) => entity.CreatedOnUtc, delegate(IMemberConfigurationExpression<ReminderRuleModel, ReminderRule, DateTime> options)
		{
			options.Ignore();
		}).ForMember((ReminderRule entity) => entity.RuleType, delegate(IMemberConfigurationExpression<ReminderRuleModel, ReminderRule, string> options)
		{
			options.Ignore();
		}).ForMember((ReminderRule entity) => entity.AvailableTokens, delegate(IMemberConfigurationExpression<ReminderRuleModel, ReminderRule, string> options)
		{
			options.MapFrom((ReminderRuleModel model) => string.Join(',', model.SelectedTokenList));
		});
		CreateMap<CustomerRemindersSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<CustomerRemindersSettings, ConfigurationModel, int> options)
		{
			options.Ignore();
		});
		CreateMap<ConfigurationModel, CustomerRemindersSettings>();
	}
}
