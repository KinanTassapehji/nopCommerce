using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Models;

public class PublicInfoModel
{
	public record ProductModel : BaseNopEntityModel
	{
		public bool HasProduct { get; set; }

		public string Name { get; set; }

		public string ShortName { get; set; }

		public string SeName { get; set; }

		public PictureModel Picture { get; set; }

		public ProductModel()
		{
			Picture = new PictureModel();
		}
	}

	public ProductModel Next { get; set; }

	public ProductModel Previous { get; set; }

	public PublicInfoModel()
	{
		Next = new ProductModel();
		Previous = new ProductModel();
	}
}
