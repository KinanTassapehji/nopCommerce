using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Services.SMS;

public interface ISmsTemplateService
{
	Task DeleteSmsTemplateAsync(SmsTemplate smsTemplate);

	Task InsertSmsTemplateAsync(SmsTemplate smsTemplate);

	Task UpdateSmsTemplateAsync(SmsTemplate smsTemplate);

	Task<SmsTemplate> GetSmsTemplateByIdAsync(int smsTemplateId);

	Task<IList<SmsTemplate>> GetAllSmsTemplatesAsync(int storeId, string keywords = null, bool? isActive = null);

	Task<IList<SmsTemplate>> GetSmsTemplatesByNameAsync(string messageTemplateName, int? storeId = null);

	Task<SmsTemplate> CopySmsTemplateAsync(SmsTemplate smsTemplate);
}
