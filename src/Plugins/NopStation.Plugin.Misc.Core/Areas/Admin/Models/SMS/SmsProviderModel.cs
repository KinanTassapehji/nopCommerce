using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;

public record SmsProviderModel : BaseNopModel, IPluginModel
{
	[NopResourceDisplayName("Admin.NopStation.Core.SmsProviders.Fields.FriendlyName")]
	public string FriendlyName { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsProviders.Fields.SystemName")]
	public string SystemName { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsProviders.Fields.DisplayOrder")]
	public int DisplayOrder { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsProviders.Fields.IsActive")]
	public bool IsActive { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsProviders.Configure")]
	public string ConfigurationUrl { get; set; }

	public string LogoUrl { get; set; }
}
