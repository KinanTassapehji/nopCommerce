using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;

public record MarketplaceCategoryModel : BaseNopModel
{
	public int Id { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Marketplace.Category.Fields.Name")]
	public string Name { get; set; }

	public int ProductCount { get; set; }
}
