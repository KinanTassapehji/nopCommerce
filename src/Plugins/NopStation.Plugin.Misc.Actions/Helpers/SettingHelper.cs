using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Internal;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure.Mapper;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.Core.Helpers;

public class SettingHelper<TSettings, TModel> : ISettingHelper<TSettings, TModel> where TSettings : class, ISettings, new() where TModel : BaseNopModel, ISettingsModel, new()
{
	private readonly ISettingService _settingService;

	private readonly IStoreContext _storeContext;

	private readonly ILocalizationService _localizationService;

	private readonly INotificationService _notificationService;

	private readonly ILanguageService _languageService;

	private Dictionary<string, List<string>> _settingsToModelMap;

	public SettingHelper(ISettingService settingService, IStoreContext storeContext, ILocalizationService localizationService, INotificationService notificationService, ILanguageService languageService)
	{
		_settingService = settingService;
		_storeContext = storeContext;
		_localizationService = localizationService;
		_notificationService = notificationService;
		_languageService = languageService;
	}

	public async Task<TSettings> LoadSettingAsync()
	{
		int storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
		return await _settingService.LoadSettingAsync<TSettings>(storeId);
	}

	public async Task<TModel> PrepareConfigurationModelAsync(Func<TModel, TSettings, Task> func = null, params string[] excludeProperties)
	{
		int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
		TSettings settings = await _settingService.LoadSettingAsync<TSettings>(storeScope);
		TModel model = AutoMapperConfiguration.Mapper.Map<TModel>(settings);
		model.ActiveStoreScopeConfiguration = storeScope;
		if (func != null)
		{
			await func(model, settings);
		}
		if (storeScope > 0)
		{
			await PopulateOverrideFlagsAsync(settings, model, storeScope, excludeProperties);
		}
		return model;
	}

	public async Task<TSettings> SaveConfigurationModelAsync(TModel model, Func<TModel, TSettings, Task> func = null, bool notifyUpdated = true, params string[] excludeProperties)
	{
		ArgumentNullException.ThrowIfNull(model, "model");
		int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
		await _settingService.LoadSettingAsync<TSettings>(storeScope);
		TSettings settings = AutoMapperConfiguration.Mapper.Map<TSettings>(model);
		await SaveWithOverrideFlagsAsync(settings, model, storeScope, excludeProperties);
		await _settingService.ClearCacheAsync();
		if (func != null)
		{
			await func(model, settings);
		}
		if (notifyUpdated)
		{
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
		}
		return settings;
	}

	private Dictionary<string, List<string>> GetSettingsToModelMap()
	{
		if (_settingsToModelMap != null)
		{
			return _settingsToModelMap;
		}
		_settingsToModelMap = BuildSourceToDestinationMap<TSettings, TModel>(AutoMapperConfiguration.Mapper);
		return _settingsToModelMap;
	}

	private Dictionary<string, List<string>> BuildSourceToDestinationMap<TSource, TDestination>(IMapper mapper)
	{
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>(StringComparer.Ordinal);
		IGlobalConfiguration globalConfiguration = mapper.ConfigurationProvider.Internal();
		TypeMap typeMap = globalConfiguration.ResolveTypeMap(typeof(TSource), typeof(TDestination)) ?? globalConfiguration.FindTypeMapFor(typeof(TSource), typeof(TDestination));
		if (typeMap == null)
		{
			return dictionary;
		}
		foreach (PropertyMap propertyMap in typeMap.PropertyMaps)
		{
			if (propertyMap == null || propertyMap.Ignored || propertyMap.DestinationMember == null)
			{
				continue;
			}
			string name = propertyMap.DestinationMember.Name;
			string text = null;
			if (propertyMap.SourceMember != null)
			{
				text = propertyMap.SourceMember.Name;
			}
			else if (propertyMap.SourceMembers != null && propertyMap.SourceMembers.Length != 0)
			{
				text = propertyMap.SourceMembers[^1]?.Name;
			}
			else if (propertyMap.CustomMapExpression != null)
			{
				MemberInfo memberInfo = TryGetSimpleSourceMember(propertyMap.CustomMapExpression);
				if (memberInfo != null)
				{
					text = memberInfo.Name;
				}
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				if (!dictionary.TryGetValue(text, out var value))
				{
					value = (dictionary[text] = new List<string>());
				}
				if (!value.Contains(name))
				{
					value.Add(name);
				}
			}
		}
		return dictionary;
	}

	private static MemberInfo TryGetSimpleSourceMember(LambdaExpression expr)
	{
		if (expr == null)
		{
			return null;
		}
		Expression expression = expr.Body;
		while (expression is UnaryExpression unaryExpression && (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked))
		{
			expression = unaryExpression.Operand;
		}
		if (!(expression is MemberExpression memberExpression))
		{
			return null;
		}
		return memberExpression.Member;
	}

	private async Task PopulateOverrideFlagsAsync(TSettings settings, TModel model, int storeScope, string[] excludeProperties)
	{
		Dictionary<string, List<string>> map = GetSettingsToModelMap();
		Type settingsType = typeof(TSettings);
		Type modelType = typeof(TModel);
		List<PropertyInfo> list = (from p in settingsType.GetProperties()
			where p.CanRead && p.CanWrite
			where !excludeProperties.Contains(p.Name)
			select p).ToList();
		foreach (PropertyInfo item in list)
		{
			if (!map.TryGetValue(item.Name, out var value))
			{
				continue;
			}
			PropertyInfo property = modelType.GetProperty(value[0]);
			if (property == null)
			{
				continue;
			}
			PropertyInfo property2 = modelType.GetProperty(property.Name + "_OverrideForStore");
			if (!(property2 == null) && !(property2.PropertyType != typeof(bool)))
			{
				ParameterExpression parameterExpression = Expression.Parameter(settingsType, "x");
				LambdaExpression lambdaExpression = Expression.Lambda(Expression.Property(parameterExpression, item), parameterExpression);
				MethodInfo methodInfo = typeof(ISettingService).GetMethod("SettingExistsAsync")?.MakeGenericMethod(settingsType, item.PropertyType);
				if (methodInfo != null)
				{
					Task<bool> obj = (Task<bool>)methodInfo.Invoke(_settingService, new object[3] { settings, lambdaExpression, storeScope });
					PropertyInfo propertyInfo = property2;
					propertyInfo.SetValue(model, await obj);
				}
			}
		}
	}

	private async Task SaveWithOverrideFlagsAsync(TSettings settings, TModel model, int storeScope, string[] excludeProperties)
	{
		Dictionary<string, List<string>> map = GetSettingsToModelMap();
		Type settingsType = typeof(TSettings);
		Type modelType = typeof(TModel);
		List<PropertyInfo> list = (from p in settingsType.GetProperties()
			where p.CanRead && p.CanWrite
			where !excludeProperties.Contains(p.Name)
			select p).ToList();
		foreach (PropertyInfo item in list)
		{
			if (!map.TryGetValue(item.Name, out var value))
			{
				continue;
			}
			PropertyInfo property = modelType.GetProperty(value[0]);
			if (!(property == null))
			{
				PropertyInfo property2 = modelType.GetProperty(property.Name + "_OverrideForStore");
				bool flag = property2 != null && property2.PropertyType == typeof(bool) && (bool)property2.GetValue(model);
				ParameterExpression parameterExpression = Expression.Parameter(settingsType, "x");
				LambdaExpression lambdaExpression = Expression.Lambda(Expression.Property(parameterExpression, item), parameterExpression);
				MethodInfo methodInfo = typeof(ISettingService).GetMethod("SaveSettingOverridablePerStoreAsync")?.MakeGenericMethod(settingsType, item.PropertyType);
				if (methodInfo != null)
				{
					await (Task)methodInfo.Invoke(_settingService, new object[5] { settings, lambdaExpression, flag, storeScope, false });
				}
			}
		}
	}
}
