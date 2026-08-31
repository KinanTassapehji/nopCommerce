using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.Product360View.Models;

public record Picture360SearchModel : BaseSearchModel
{
	public int ProductId { get; set; }

	public bool IsPanorama { get; set; }
}
