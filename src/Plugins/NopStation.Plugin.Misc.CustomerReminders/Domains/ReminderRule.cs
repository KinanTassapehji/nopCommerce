using System;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.Misc.CustomerReminders.Domains;

public class ReminderRule : BaseEntity, ISoftDeletedEntity
{
	public string SystemName { get; set; }

	public string Description { get; set; }

	public bool IsEnabled { get; set; }

	public string AvailableTokens { get; set; }

	public string RuleType { get; set; }

	public DateTime CreatedOnUtc { get; set; }

	public bool Deleted { get; set; }
}
