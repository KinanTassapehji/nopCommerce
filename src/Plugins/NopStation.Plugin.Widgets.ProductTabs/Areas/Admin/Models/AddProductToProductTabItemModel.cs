using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;

public record AddProductToProductTabItemModel : BaseNopModel
{
	public int ProductTabItemId { get; set; }

	public IList<int> SelectedProductIds { get; set; }

	public AddProductToProductTabItemModel()
	{
		SelectedProductIds = new List<int>();
	}
}
