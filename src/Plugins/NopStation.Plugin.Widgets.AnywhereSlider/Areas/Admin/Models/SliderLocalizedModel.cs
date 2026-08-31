using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;

public class SliderLocalizedModel : ILocalizedLocaleModel
{
	public int LanguageId { get; set; }

	[NopResourceDisplayName("Admin.NopStation.AnywhereSlider.Sliders.Fields.Name")]
	public string Name { get; set; }
}
