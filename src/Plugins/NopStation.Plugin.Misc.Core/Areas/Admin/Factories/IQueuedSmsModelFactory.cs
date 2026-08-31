using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Factories;

public interface IQueuedSmsModelFactory
{
	QueuedSmsSearchModel PrepareQueuedSmsSearchModel(QueuedSmsSearchModel searchModel);

	Task<QueuedSmsListModel> PrepareQueuedSmsListModelAsync(QueuedSmsSearchModel searchModel);

	Task<QueuedSmsModel> PrepareQueuedSmsModelAsync(QueuedSmsModel model, QueuedSms queuedSms, bool excludeProperties = false);
}
