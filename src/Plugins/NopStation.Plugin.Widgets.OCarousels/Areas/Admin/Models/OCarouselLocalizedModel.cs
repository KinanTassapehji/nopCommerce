using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models;

public class OCarouselLocalizedModel : ILocalizedLocaleModel
{
	public int LanguageId { get; set; }

	[NopResourceDisplayName("Admin.NopStation.OCarousels.OCarousels.Fields.Title")]
	public string Title { get; set; }
}
