using System;
using System.Threading.Tasks;
using Nop.Core.Configuration;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.Core.Helpers;

public interface ISettingHelper<TSettings, TModel> where TSettings : class, ISettings, new() where TModel : BaseNopModel, ISettingsModel, new()
{
	Task<TSettings> LoadSettingAsync();

	Task<TModel> PrepareConfigurationModelAsync(Func<TModel, TSettings, Task> func = null, params string[] excludeProperties);

	Task<TSettings> SaveConfigurationModelAsync(TModel model, Func<TModel, TSettings, Task> func = null, bool notifyUpdated = true, params string[] excludeProperties);
}
