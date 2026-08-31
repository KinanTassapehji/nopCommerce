using System.Threading.Tasks;
using NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.CustomerReminders.Areas.Admin.Factories;

public interface IConfigurationModelFactory
{
	Task<ConfigurationModel> PrepareConfigurationModelAsync();
}
