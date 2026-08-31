using System.Threading.Tasks;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Discounts;
using Nop.Core.Events;
using Nop.Services.Configuration;
using Nop.Services.Events;

namespace NopStation.Plugin.DiscountRules.CartTotal.Infrastructure.Cache;

public class DiscountRequirementCartTotalEventConsumer : IConsumer<EntityDeletedEvent<DiscountRequirement>>
{
	private readonly ISettingService _settingService;

	public DiscountRequirementCartTotalEventConsumer(ISettingService settingService)
	{
		_settingService = settingService;
	}

	public async Task HandleEventAsync(EntityDeletedEvent<DiscountRequirement> eventMessage)
	{
		DiscountRequirement discountRequirement = eventMessage?.Entity;
		if (discountRequirement != null)
		{
			Setting setting = await _settingService.GetSettingAsync(string.Format(DiscountRequirementDefaults.MinimumCartTotalSettingsKey, discountRequirement.Id));
			if (setting != null)
			{
				await _settingService.DeleteSettingAsync(setting);
			}
		}
	}
}
