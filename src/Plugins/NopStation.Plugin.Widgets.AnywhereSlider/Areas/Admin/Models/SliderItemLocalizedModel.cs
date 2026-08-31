using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;

public class SliderItemLocalizedModel : ILocalizedLocaleModel
{
	public int LanguageId { get; set; }

	[NopResourceDisplayName("Admin.NopStation.AnywhereSlider.SliderItems.Fields.Title")]
	public string SliderItemTitle { get; set; }

	[NopResourceDisplayName("Admin.NopStation.AnywhereSlider.SliderItems.Fields.ShortDescription")]
	public string ShortDescription { get; set; }

	[NopResourceDisplayName("Admin.NopStation.AnywhereSlider.SliderItems.Fields.ImageAltText")]
	public string ImageAltText { get; set; }

	[NopResourceDisplayName("Admin.NopStation.AnywhereSlider.SliderItems.Fields.Link")]
	public string Link { get; set; }

	[NopResourceDisplayName("Admin.NopStation.AnywhereSlider.SliderItems.Fields.ShopNowLink")]
	public string ShopNowLink { get; set; }
}
