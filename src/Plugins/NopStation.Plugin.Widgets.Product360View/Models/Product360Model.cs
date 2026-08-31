using System.Collections.Generic;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Widgets.Product360View.Models;

public record Product360Model : BaseNopEntityModel
{
	public ProductPictureModel AddPictureModel { get; set; }

	public Picture360SearchModel ProductPictureSearchModel { get; set; }

	public ImageSetting360Model ImageSetting360Model { get; set; }

	public List<string> PictureUrls { get; set; }

	public List<string> PanoramaPictureUrls { get; set; }

	public List<PictureModel> PictureModels { get; set; }

	public Product360Model()
	{
		AddPictureModel = new ProductPictureModel();
		ProductPictureSearchModel = new Picture360SearchModel();
		ImageSetting360Model = new ImageSetting360Model();
		PictureUrls = new List<string>();
		PanoramaPictureUrls = new List<string>();
		PictureModels = new List<PictureModel>();
	}
}
