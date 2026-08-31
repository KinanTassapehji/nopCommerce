using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Factories;

public interface ISmsModelFactory
{
	Task<SmsProviderSearchModel> PrepareSmsProviderSearchModelAsync(SmsProviderSearchModel searchModel);

	Task<SmsProviderListModel> PrepareSmsProviderListModelAsync(SmsProviderSearchModel searchModel);
}
