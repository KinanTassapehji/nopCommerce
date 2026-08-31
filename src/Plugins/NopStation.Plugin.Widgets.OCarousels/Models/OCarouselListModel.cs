using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.OCarousels.Models;

public record OCarouselListModel : BaseNopModel
{
	public record OCarouselOverviewModel : BaseNopEntityModel
	{
		public string Title { get; set; }

		public bool DisplayTitle { get; set; }

		public bool ShowBackgroundPicture { get; set; }

		public string BackgroundPictureUrl { get; set; }

		public CarouselType CarouselType { get; set; }
	}

	public List<OCarouselOverviewModel> OCarousels { get; set; }

	public OCarouselListModel()
	{
		OCarousels = new List<OCarouselOverviewModel>();
	}
}
