using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Factories;

public interface ISmsTemplateModelFactory
{
	SmsTemplateSearchModel PrepareSmsTemplateSearchModel(SmsTemplateSearchModel searchModel);

	Task<SmsTemplateListModel> PrepareSmsTemplateListModelAsync(SmsTemplateSearchModel searchModel);

	Task<SmsTemplateModel> PrepareSmsTemplateModelAsync(SmsTemplateModel model, SmsTemplate smsTemplate, bool excludeProperties = false);
}
