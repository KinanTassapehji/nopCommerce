using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Services.SMS;

public class SmsTemplateService : ISmsTemplateService
{
	private readonly IRepository<SmsTemplate> _smsTemplateRepository;

	private readonly CatalogSettings _catalogSettings;

	private readonly IRepository<StoreMapping> _storeMappingRepository;

	private readonly IStoreMappingService _storeMappingService;

	private readonly IStaticCacheManager _cacheManager;

	private readonly ILanguageService _languageService;

	private readonly ILocalizationService _localizationService;

	private readonly ILocalizedEntityService _localizedEntityService;

	public SmsTemplateService(IRepository<SmsTemplate> smsTemplateRepository, IStaticCacheManager cacheManager, CatalogSettings catalogSettings, IRepository<StoreMapping> storeMappingRepository, IStoreMappingService storeMappingService, ILanguageService languageService, ILocalizationService localizationService, ILocalizedEntityService localizedEntityService)
	{
		_smsTemplateRepository = smsTemplateRepository;
		_cacheManager = cacheManager;
		_catalogSettings = catalogSettings;
		_storeMappingRepository = storeMappingRepository;
		_storeMappingService = storeMappingService;
		_languageService = languageService;
		_localizationService = localizationService;
		_localizedEntityService = localizedEntityService;
	}

	public async Task DeleteSmsTemplateAsync(SmsTemplate smsTemplate)
	{
		await _smsTemplateRepository.DeleteAsync(smsTemplate);
		await _cacheManager.RemoveByPrefixAsync(SmsDefaults.MessageTemplatesPrefixCacheKey);
	}

	public async Task InsertSmsTemplateAsync(SmsTemplate smsTemplate)
	{
		await _smsTemplateRepository.InsertAsync(smsTemplate);
		await _cacheManager.RemoveByPrefixAsync(SmsDefaults.MessageTemplatesPrefixCacheKey);
	}

	public async Task UpdateSmsTemplateAsync(SmsTemplate smsTemplate)
	{
		await _smsTemplateRepository.UpdateAsync(smsTemplate);
		await _cacheManager.RemoveByPrefixAsync(SmsDefaults.MessageTemplatesPrefixCacheKey);
	}

	public async Task<SmsTemplate> GetSmsTemplateByIdAsync(int smsTemplateId)
	{
		if (smsTemplateId == 0)
		{
			return null;
		}
		return await _smsTemplateRepository.GetByIdAsync(smsTemplateId, (ICacheKeyService cache) => (CacheKey)null);
	}

	public async Task<IList<SmsTemplate>> GetAllSmsTemplatesAsync(int storeId, string keywords = null, bool? isActive = null)
	{
		CacheKey cacheKey = new CacheKey(SmsDefaults.MessageTemplatesAllCacheKey);
		CacheKey key = _cacheManager.PrepareKeyForDefaultCache(cacheKey, storeId, keywords ?? string.Empty, isActive?.ToString() ?? string.Empty);
		return await _cacheManager.GetAsync(key, delegate
		{
			IQueryable<SmsTemplate> source = _smsTemplateRepository.Table;
			if (!string.IsNullOrWhiteSpace(keywords))
			{
				source = source.Where((SmsTemplate t) => t.Name.Contains(keywords) || t.Body.Contains(keywords));
			}
			if (isActive.HasValue)
			{
				source = source.Where((SmsTemplate t) => t.Active == ((bool?)isActive).Value);
			}
			source = source.OrderBy((SmsTemplate t) => t.Name);
			if (storeId <= 0 || _catalogSettings.IgnoreStoreLimitations)
			{
				return source.ToList();
			}
			source = from t in source
				join sm in _storeMappingRepository.Table on new
				{
					c1 = t.Id,
					c2 = "SmsTemplate"
				} equals new
				{
					c1 = sm.EntityId,
					c2 = sm.EntityName
				} into tSm
				from sm in tSm.DefaultIfEmpty()
				where !t.LimitedToStores || storeId == sm.StoreId
				select t;
			source = from t in source.Distinct()
				orderby t.Name
				select t;
			return source.ToList();
		});
	}

	public async Task<IList<SmsTemplate>> GetSmsTemplatesByNameAsync(string messageTemplateName, int? storeId = null)
	{
		if (string.IsNullOrWhiteSpace(messageTemplateName))
		{
			throw new ArgumentException("messageTemplateName");
		}
		CacheKey cacheKey = new CacheKey(SmsDefaults.MessageTemplatesByNameCacheKey);
		CacheKey key = _cacheManager.PrepareKeyForDefaultCache(cacheKey, messageTemplateName, storeId.GetValueOrDefault());
		return await _cacheManager.GetAsync(key, async delegate
		{
			List<SmsTemplate> list = (from messageTemplate in _smsTemplateRepository.Table
				where messageTemplate.Name.Equals(messageTemplateName)
				orderby messageTemplate.Id
				select messageTemplate).ToList();
			if (storeId.HasValue && storeId.Value > 0)
			{
				list = await list.WhereAwait(async (SmsTemplate messageTemplate) => await _storeMappingService.AuthorizeAsync(messageTemplate, storeId.Value)).ToListAsync();
			}
			return list;
		});
	}

	public async Task<SmsTemplate> CopySmsTemplateAsync(SmsTemplate smsTemplate)
	{
		ArgumentNullException.ThrowIfNull(smsTemplate, "smsTemplate");
		SmsTemplate copy = new SmsTemplate
		{
			Name = smsTemplate.Name,
			Body = smsTemplate.Body,
			Active = smsTemplate.Active,
			LimitedToStores = smsTemplate.LimitedToStores,
			SubjectToAcl = smsTemplate.SubjectToAcl,
			ProviderSystemName = smsTemplate.ProviderSystemName
		};
		await InsertSmsTemplateAsync(copy);
		foreach (Language lang in await _languageService.GetAllLanguagesAsync(showHidden: true))
		{
			string text = await _localizationService.GetLocalizedAsync(smsTemplate, (SmsTemplate x) => x.Body, lang.Id, returnDefaultValue: false, ensureTwoPublishedLanguages: false);
			if (!string.IsNullOrEmpty(text))
			{
				await _localizedEntityService.SaveLocalizedValueAsync(copy, (SmsTemplate x) => x.Body, text, lang.Id);
			}
		}
		int[] array = await _storeMappingService.GetStoresIdsWithAccessAsync(smsTemplate);
		foreach (int storeId in array)
		{
			await _storeMappingService.InsertStoreMappingAsync(copy, storeId);
		}
		return copy;
	}
}
