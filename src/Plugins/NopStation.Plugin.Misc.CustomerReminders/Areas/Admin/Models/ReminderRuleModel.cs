using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

public record ReminderRuleModel : BaseNopEntityModel
{
	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.SystemName")]
	public string SystemName { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.Description")]
	public string Description { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.IsEnabled")]
	public bool IsEnabled { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.AvailableTokens")]
	public IList<string> SelectedTokenList { get; set; }

	public IList<SelectListItem> AvailableTokens { get; set; }

	[NopResourceDisplayName("NopStation.Plugins.CustomerReminders.Admin.ReminderRules.Fields.RuleType")]
	public string RuleType { get; set; }

	public ReminderRuleModel()
	{
		AvailableTokens = new List<SelectListItem>();
		SelectedTokenList = new List<string>();
	}
}
