using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;

public class ProductTabLocalizedModel : ILocalizedLocaleModel
{
	public int LanguageId { get; set; }

	[NopResourceDisplayName("Admin.NopStation.ProductTabs.ProductTabs.Fields.Name")]
	public string Name { get; set; }

	[NopResourceDisplayName("Admin.NopStation.ProductTabs.ProductTabs.Fields.Title")]
	public string TabTitle { get; set; }
}
