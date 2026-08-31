using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;

public record MarketplaceSearchModel : BaseNopModel
{
	public int CategoryId { get; set; }

	public int PaidFilter { get; set; }

	public string SearchText { get; set; }

	public int VersionFilter { get; set; }

	public int PageNumber { get; set; } = 1;
}
