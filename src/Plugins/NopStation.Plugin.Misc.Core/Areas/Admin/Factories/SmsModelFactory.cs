using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Plugins;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;
using NopStation.Plugin.Misc.Core.Services.SMS;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Factories;

public class SmsModelFactory : ISmsModelFactory
{
	private readonly ISmsPluginManager _smsPluginManager;

	private readonly IPluginService _pluginService;

	public SmsModelFactory(ISmsPluginManager smsPluginManager, IPluginService pluginService)
	{
		_smsPluginManager = smsPluginManager;
		_pluginService = pluginService;
	}

	public virtual Task<SmsProviderSearchModel> PrepareSmsProviderSearchModelAsync(SmsProviderSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		searchModel.SetGridPageSize();
		return Task.FromResult(searchModel);
	}

	public virtual async Task<SmsProviderListModel> PrepareSmsProviderListModelAsync(SmsProviderSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(searchModel, "searchModel");
		IPagedList<ISmsPlugin> smsPlugins = (await _smsPluginManager.LoadSmsPluginsAsync()).ToList().ToPagedList(searchModel);
		return await new SmsProviderListModel().PrepareToGridAsync(searchModel, smsPlugins, () => smsPlugins.SelectAwait<ISmsPlugin, SmsProviderModel>(async delegate(ISmsPlugin plugin)
		{
			SmsProviderModel providerModel = plugin.ToPluginModel<SmsProviderModel>();
			SmsProviderModel smsProviderModel = providerModel;
			smsProviderModel.IsActive = await plugin.IsActiveAsync();
			providerModel.ConfigurationUrl = plugin.GetConfigurationPageUrl();
			smsProviderModel = providerModel;
			smsProviderModel.LogoUrl = await _pluginService.GetPluginLogoUrlAsync(plugin.PluginDescriptor);
			return providerModel;
		}));
	}
}
