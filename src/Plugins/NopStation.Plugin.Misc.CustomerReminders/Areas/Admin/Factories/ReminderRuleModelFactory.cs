using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;
using NopStation.Plugin.Misc.CustomerReminders.Domains;
using NopStation.Plugin.Misc.CustomerReminders.Helpers;
using NopStation.Plugin.Misc.CustomerReminders.Services;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;

public class ReminderRuleModelFactory : IReminderRuleModelFactory
{
	private readonly IReminderRuleService _reminderRuleService;

	public ReminderRuleModelFactory(IReminderRuleService reminderRuleService)
	{
		_reminderRuleService = reminderRuleService;
	}

	public virtual Task<ReminderRuleSearchModel> PrepareReminderRuleSearchModelAsync(ReminderRuleSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		searchModel.SetGridPageSize();
		return Task.FromResult(searchModel);
	}

	public virtual async Task<ReminderRuleListModel> PrepareReminderRuleListModelAsync(ReminderRuleSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		IPagedList<ReminderRule> reminderRules = await _reminderRuleService.GetAllReminderRulesAsync(showHidden: false, searchModel.Page - 1, searchModel.PageSize);
		return new ReminderRuleListModel().PrepareToGrid(searchModel, reminderRules, () => reminderRules.Select((ReminderRule reminderRule) => reminderRule.ToModel<ReminderRuleModel>()));
	}

	public virtual ReminderRuleModel PrepareReminderRuleModel(ReminderRuleModel model, ReminderRule reminderRule, bool excludeProperties = false)
	{
		if (model == null)
		{
			model = ((reminderRule == null || excludeProperties) ? new ReminderRuleModel() : reminderRule.ToModel<ReminderRuleModel>());
		}
		if ((object)model == null)
		{
			model = new ReminderRuleModel();
		}
		if (reminderRule != null && !string.IsNullOrEmpty(reminderRule.AvailableTokens))
		{
			model.SelectedTokenList = ReminderRuleTokenGroupHelper.ParseTokenGroups(reminderRule.AvailableTokens);
		}
		model.AvailableTokens = ReminderRuleTokenGroupHelper.GetAvailableTokenGroups();
		return model;
	}
}
