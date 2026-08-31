using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;

public record ProductTabItemModel : BaseNopEntityModel, ILocalizedModel<ProductTabItemLocalizedModel>, ILocalizedModel
{
	[NopResourceDisplayName("Admin.NopStation.ProductTabs.ProductTabItems.Fields.Name")]
	public string Name { get; set; }

	[NopResourceDisplayName("Admin.NopStation.ProductTabs.ProductTabItems.Fields.DisplayOrder")]
	public int DisplayOrder { get; set; }

	[NopResourceDisplayName("Admin.NopStation.ProductTabs.ProductTabItems.Fields.ProductTab")]
	public int ProductTabId { get; set; }

	public IList<ProductTabItemLocalizedModel> Locales { get; set; }

	public ProductTabItemProductSearchModel ProductSearchModel { get; set; }

	public ProductTabItemModel()
	{
		ProductSearchModel = new ProductTabItemProductSearchModel();
		Locales = new List<ProductTabItemLocalizedModel>();
	}
}
