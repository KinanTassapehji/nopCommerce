using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;
using NopStation.Plugin.Misc.Core.Domains.SMS;
using NopStation.Plugin.Misc.Core.Services.SMS;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Factories;

public class SmsTemplateModelFactory : ISmsTemplateModelFactory
{
	private readonly ILocalizationService _localizationService;

	private readonly ISmsTemplateService _smsTemplateService;

	private readonly ILocalizedModelFactory _localizedModelFactory;

	private readonly ISmsTokenProvider _smsTokenProvider;

	private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

	private readonly IAclSupportedModelFactory _aclSupportedModelFactory;

	private readonly ISmsPluginManager _smsPluginManager;

	private readonly IStoreService _storeService;

	public SmsTemplateModelFactory(ILocalizationService localizationService, ISmsTemplateService smsTemplateService, ILocalizedModelFactory localizedModelFactory, ISmsTokenProvider smsTokenProvider, IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory, IAclSupportedModelFactory aclSupportedModelFactory, ISmsPluginManager smsPluginManager, IStoreService storeService)
	{
		_localizationService = localizationService;
		_smsTemplateService = smsTemplateService;
		_localizedModelFactory = localizedModelFactory;
		_smsTokenProvider = smsTokenProvider;
		_storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
		_aclSupportedModelFactory = aclSupportedModelFactory;
		_smsPluginManager = smsPluginManager;
		_storeService = storeService;
	}

	public virtual SmsTemplateSearchModel PrepareSmsTemplateSearchModel(SmsTemplateSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		searchModel.SetGridPageSize();
		return searchModel;
	}

	public virtual async Task<SmsTemplateListModel> PrepareSmsTemplateListModelAsync(SmsTemplateSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		bool? isActive = ((searchModel.SearchActiveId == 0) ? ((bool?)null) : new bool?(searchModel.SearchActiveId == 1));
		IPagedList<SmsTemplate> smsTemplates = (await _smsTemplateService.GetAllSmsTemplatesAsync(0, searchModel.SearchKeywords, isActive)).ToPagedList(searchModel);
		var stores = (await _storeService.GetAllStoresAsync()).Select((Store store) => new { store.Id, store.Name }).ToList();
		return await new SmsTemplateListModel().PrepareToGridAsync(searchModel, smsTemplates, () => smsTemplates.SelectAwait<SmsTemplate, SmsTemplateModel>(async delegate(SmsTemplate smsTemplate)
		{
			SmsTemplateModel smsTemplateModel = smsTemplate.ToModel<SmsTemplateModel>();
			if (smsTemplate.LimitedToStores)
			{
				await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(smsTemplateModel, smsTemplate, ignoreStoreMappings: false);
				IEnumerable<string> values = from store in stores
					where smsTemplateModel.SelectedStoreIds.Contains(store.Id)
					select store.Name;
				smsTemplateModel.ListOfStores = string.Join(", ", values);
			}
			else
			{
				string listOfStores = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.AllSettings.Fields.StoreName.AllStores");
				smsTemplateModel.ListOfStores = listOfStores;
			}
			return smsTemplateModel;
		}));
	}

	public virtual async Task<SmsTemplateModel> PrepareSmsTemplateModelAsync(SmsTemplateModel model, SmsTemplate smsTemplate, bool excludeProperties = false)
	{
		Func<SmsTemplateLocalizedModel, int, Task> localizedModelConfiguration = null;
		if (smsTemplate != null)
		{
			model = model ?? smsTemplate.ToModel<SmsTemplateModel>();
			model.Name = smsTemplate.Name;
		}
		if (!excludeProperties)
		{
			IEnumerable<string> enumerable;
			if (smsTemplate != null)
			{
				enumerable = _smsTokenProvider.GetTokenGroups(smsTemplate);
			}
			else
			{
				IEnumerable<string> enumerable2 = Array.Empty<string>();
				enumerable = enumerable2;
			}
			IEnumerable<string> tokenGroups = enumerable;
			string text = string.Join(", ", _smsTokenProvider.GetListOfAllowedTokens(tokenGroups));
			SmsTemplateModel smsTemplateModel = model;
			string text2 = text;
			string newLine = Environment.NewLine;
			string newLine2 = Environment.NewLine;
			string text3 = await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Tokens.ConditionalStatement");
			smsTemplateModel.AllowedTokens = text2 + newLine + newLine2 + text3 + Environment.NewLine;
			smsTemplateModel = model;
			smsTemplateModel.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
			IList<ISmsPlugin> smsPlugins = await _smsPluginManager.LoadSmsPluginsAsync();
			IList<SelectListItem> availableSmsProviders = model.AvailableSmsProviders;
			SelectListItem selectListItem = new SelectListItem();
			SelectListItem selectListItem2 = selectListItem;
			selectListItem2.Text = await _localizationService.GetResourceAsync("Admin.Common.Select");
			selectListItem.Value = string.Empty;
			availableSmsProviders.Add(selectListItem);
			foreach (ISmsPlugin item in smsPlugins)
			{
				model.AvailableSmsProviders.Add(new SelectListItem
				{
					Text = item.PluginDescriptor.FriendlyName,
					Value = item.PluginDescriptor.SystemName,
					Selected = (item.PluginDescriptor.SystemName == model.ProviderSystemName)
				});
			}
		}
		await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, "SmsTemplate");
		await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, smsTemplate, excludeProperties);
		return model;
	}
}
