using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.OCarousels.Domains;
using NopStation.Plugin.Widgets.OCarousels.Factories;
using NopStation.Plugin.Widgets.OCarousels.Models;
using NopStation.Plugin.Widgets.OCarousels.Services;

namespace NopStation.Plugin.Widgets.OCarousels.Controllers;

public class OCarouselController : NopStationPublicController
{
	private readonly IOCarouselModelFactory _carouselModelFactory;

	private readonly IOCarouselService _carouselService;

	public OCarouselController(IOCarouselModelFactory carouselModelFactory, IOCarouselService carouselService)
	{
		_carouselModelFactory = carouselModelFactory;
		_carouselService = carouselService;
	}

	[HttpPost]
	public async Task<IActionResult> Details(int carouselId)
	{
		OCarousel oCarousel = await _carouselService.GetCarouselByIdAsync(carouselId);
		if (oCarousel == null || oCarousel.Deleted || !oCarousel.Active)
		{
			return Json(new
			{
				result = false
			});
		}
		OCarouselModel oCarouselModel = await _carouselModelFactory.PrepareCarouselModelAsync(oCarousel);
		return Json(new
		{
			result = true,
			html = await RenderPartialViewToStringAsync(oCarouselModel.CarouselType.ToString(), oCarouselModel),
			carouselid = carouselId
		});
	}
}
