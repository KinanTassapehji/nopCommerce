using System.Collections.Generic;
using Nop.Web.Framework.UI.Paging;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;

public record MarketplaceModel : BasePageableModel
{
	public IList<MarketplaceCategoryModel> Categories { get; set; }

	public IList<MarketplaceProductModel> Products { get; set; }

	public int TotalCount { get; set; }

	public int ActiveCategoryId { get; set; }

	public int ActivePaidFilter { get; set; }

	public string ActiveSearchText { get; set; }

	public int ActiveVersionFilter { get; set; }

	public string MarketplaceLogoUrl { get; set; }

	public MarketplaceModel()
	{
		Categories = new List<MarketplaceCategoryModel>();
		Products = new List<MarketplaceProductModel>();
	}
}
