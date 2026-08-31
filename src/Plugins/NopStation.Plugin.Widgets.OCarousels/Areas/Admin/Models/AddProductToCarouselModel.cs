using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models;

public record AddProductToCarouselModel : BaseNopModel
{
	public int OCarouselId { get; set; }

	public IList<int> SelectedProductIds { get; set; }

	public AddProductToCarouselModel()
	{
		SelectedProductIds = new List<int>();
	}
}
