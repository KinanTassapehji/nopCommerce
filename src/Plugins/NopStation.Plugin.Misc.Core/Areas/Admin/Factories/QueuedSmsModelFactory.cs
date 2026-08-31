using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;
using NopStation.Plugin.Misc.Core.Domains.SMS;
using NopStation.Plugin.Misc.Core.Services.SMS;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Factories;

public class QueuedSmsModelFactory : IQueuedSmsModelFactory
{
	private readonly IDateTimeHelper _dateTimeHelper;

	private readonly ILocalizationService _localizationService;

	private readonly IQueuedSmsService _queuedSmsService;

	private readonly ICustomerService _customerService;

	private readonly IStoreService _storeService;

	public QueuedSmsModelFactory(IDateTimeHelper dateTimeHelper, ILocalizationService localizationService, IQueuedSmsService queuedSmsService, ICustomerService customerService, IStoreService storeService)
	{
		_dateTimeHelper = dateTimeHelper;
		_localizationService = localizationService;
		_queuedSmsService = queuedSmsService;
		_customerService = customerService;
		_storeService = storeService;
	}

	public virtual QueuedSmsSearchModel PrepareQueuedSmsSearchModel(QueuedSmsSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		searchModel.SearchMaxSentTries = 10;
		searchModel.SetGridPageSize();
		return searchModel;
	}

	public virtual async Task<QueuedSmsListModel> PrepareQueuedSmsListModelAsync(QueuedSmsSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		DateTime? dateTime;
		if (!searchModel.SearchStartDate.HasValue)
		{
			dateTime = null;
		}
		else
		{
			IDateTimeHelper dateTimeHelper = _dateTimeHelper;
			DateTime value = searchModel.SearchStartDate.Value;
			dateTime = dateTimeHelper.ConvertToUtcTime(value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
		}
		DateTime? startDateValue = dateTime;
		if (!searchModel.SearchEndDate.HasValue)
		{
			dateTime = null;
		}
		else
		{
			IDateTimeHelper dateTimeHelper = _dateTimeHelper;
			DateTime value = searchModel.SearchEndDate.Value;
			dateTime = dateTimeHelper.ConvertToUtcTime(value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1.0);
		}
		DateTime? createdToUtc = dateTime;
		IPagedList<QueuedSms> queuedSmss = await _queuedSmsService.GetAllQueuedSmsAsync(searchModel.SearchLoadNotSent, searchModel.SearchMaxSentTries, searchModel.SearchPhoneNumber, startDateValue, createdToUtc, searchModel.Page - 1, searchModel.PageSize);
		return await new QueuedSmsListModel().PrepareToGridAsync(searchModel, queuedSmss, () => queuedSmss.SelectAwait<QueuedSms, QueuedSmsModel>(async (QueuedSms queuedSms) => await PrepareQueuedSmsModelAsync(null, queuedSms)));
	}

	public virtual async Task<QueuedSmsModel> PrepareQueuedSmsModelAsync(QueuedSmsModel model, QueuedSms queuedSms, bool excludeProperties = false)
	{
		if (queuedSms != null)
		{
			model = model ?? queuedSms.ToModel<QueuedSmsModel>();
			QueuedSmsModel queuedSmsModel = model;
			queuedSmsModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(queuedSms.CreatedOnUtc, DateTimeKind.Utc);
			if (queuedSms.SentOnUtc.HasValue)
			{
				queuedSmsModel = model;
				queuedSmsModel.SentOn = await _dateTimeHelper.ConvertToUserTimeAsync(queuedSms.SentOnUtc.Value, DateTimeKind.Utc);
			}
			if (!string.IsNullOrWhiteSpace(model.Body))
			{
				model.Body = model.Body.Replace(Environment.NewLine, "<br />");
			}
			if (!queuedSms.CustomerId.HasValue || queuedSms.CustomerId == 0)
			{
				queuedSmsModel = model;
				queuedSmsModel.CustomerName = await _localizationService.GetResourceAsync("Admin.NopStation.Core.QueuedSms.All");
			}
			else
			{
				Customer customer = await _customerService.GetCustomerByIdAsync(queuedSms.CustomerId.Value);
				if (customer == null)
				{
					model.CustomerId = 0;
					queuedSmsModel = model;
					queuedSmsModel.CustomerName = await _localizationService.GetResourceAsync("Admin.NopStation.Core.QueuedSms.Unknown");
				}
				else if (await _customerService.IsRegisteredAsync(customer))
				{
					model.CustomerName = customer.Email;
				}
				else
				{
					model.CustomerName = "Guest";
				}
			}
			Store store = await _storeService.GetStoreByIdAsync(queuedSms.StoreId);
			if (store != null)
			{
				model.StoreName = store.Name;
			}
		}
		return model;
	}
}
