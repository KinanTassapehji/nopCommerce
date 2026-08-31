using System.Threading.Tasks;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Discounts;
using Nop.Core.Events;
using Nop.Services.Configuration;
using Nop.Services.Events;

namespace NopStation.Plugin.DiscountRules.OrderRange.Infrastructure.Cache;

public class DiscountRequirementRangerEventConsumer : IConsumer<EntityDeletedEvent<DiscountRequirement>>
{
	private readonly ISettingService _settingService;

	public DiscountRequirementRangerEventConsumer(ISettingService settingService)
	{
		_settingService = settingService;
	}

	public async Task HandleEventAsync(EntityDeletedEvent<DiscountRequirement> eventMessage)
	{
		DiscountRequirement discountRequirement = eventMessage?.Entity;
		if (discountRequirement != null)
		{
			Setting setting = await _settingService.GetSettingAsync(string.Format(DiscountRequirementDefaults.ConditionValueSettingsKey, discountRequirement.Id));
			if (setting != null)
			{
				await _settingService.DeleteSettingAsync(setting);
			}
			setting = await _settingService.GetSettingAsync(string.Format(DiscountRequirementDefaults.RangeValueSettingsKey, discountRequirement.Id));
			if (setting != null)
			{
				await _settingService.DeleteSettingAsync(setting);
			}
		}
	}
}
