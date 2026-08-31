using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;

public record CoreLocaleResourceModel : BaseNopEntityModel
{
	[NopResourceDisplayName("Admin.NopStation.Core.Resources.Fields.Name")]
	public string ResourceName { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Resources.Fields.Value")]
	public string ResourceValue { get; set; }

	public string ResourceNameLanguageId { get; set; }

	public int LanguageId { get; set; }
}
