using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;

public class ProductTabItemLocalizedModel : ILocalizedLocaleModel
{
	public int LanguageId { get; set; }

	[NopResourceDisplayName("Admin.NopStation.ProductTabs.ProductTabItems.Fields.Name")]
	public string Name { get; set; }
}
