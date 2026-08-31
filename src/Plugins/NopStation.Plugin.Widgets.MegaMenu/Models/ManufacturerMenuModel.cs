using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Widgets.MegaMenu.Models;

public record ManufacturerMenuModel : BaseNopEntityModel
{
	public string Name { get; set; }

	public string SeName { get; set; }

	public PictureModel PictureModel { get; set; }

	public ManufacturerMenuModel()
	{
		PictureModel = new PictureModel();
	}
}
