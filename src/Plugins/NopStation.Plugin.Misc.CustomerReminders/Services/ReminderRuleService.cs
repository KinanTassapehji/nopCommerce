using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Services;

public class ReminderRuleService : IReminderRuleService
{
	private readonly IRepository<ReminderRule> _reminderRuleRepository;

	public ReminderRuleService(IRepository<ReminderRule> reminderRuleRepository)
	{
		_reminderRuleRepository = reminderRuleRepository;
	}

	public virtual async Task<ReminderRule> GetReminderRuleByIdAsync(int reminderRuleId)
	{
		if (reminderRuleId == 0)
		{
			return null;
		}
		return await _reminderRuleRepository.GetByIdAsync(reminderRuleId);
	}

	public virtual async Task<ReminderRule> GetReminderRuleBySystemNameAsync(string systemName)
	{
		if (string.IsNullOrWhiteSpace(systemName))
		{
			return null;
		}
		return await _reminderRuleRepository.Table.Where((ReminderRule rr) => rr.SystemName == systemName && !rr.Deleted).FirstOrDefaultAsync();
	}

	public virtual async Task<IPagedList<ReminderRule>> GetAllReminderRulesAsync(bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
	{
		IQueryable<ReminderRule> source = _reminderRuleRepository.Table;
		if (!showHidden)
		{
			source = source.Where((ReminderRule rr) => !rr.Deleted);
		}
		source = source.OrderBy((ReminderRule rr) => rr.SystemName);
		return await source.ToPagedListAsync(pageIndex, pageSize);
	}

	public virtual async Task InsertReminderRuleAsync(ReminderRule reminderRule)
	{
		ArgumentNullException.ThrowIfNull(reminderRule, "reminderRule");
		reminderRule.CreatedOnUtc = DateTime.UtcNow;
		reminderRule.RuleType = "Custom";
		await _reminderRuleRepository.InsertAsync(reminderRule);
	}

	public virtual async Task UpdateReminderRuleAsync(ReminderRule reminderRule)
	{
		ArgumentNullException.ThrowIfNull(reminderRule, "reminderRule");
		await _reminderRuleRepository.UpdateAsync(reminderRule);
	}

	public virtual async Task DeleteReminderRuleAsync(ReminderRule reminderRule)
	{
		ArgumentNullException.ThrowIfNull(reminderRule, "reminderRule");
		reminderRule.Deleted = true;
		await _reminderRuleRepository.UpdateAsync(reminderRule);
	}

	public virtual async Task<bool> IsSystemNameUniqueAsync(string systemName, int currentReminderRuleId = 0)
	{
		if (string.IsNullOrWhiteSpace(systemName))
		{
			return false;
		}
		return !(await _reminderRuleRepository.Table.Where((ReminderRule rr) => rr.Id != currentReminderRuleId && rr.SystemName.Trim().ToLower() == systemName.Trim().ToLower() && !rr.Deleted).AnyAsync());
	}
}
