using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;
using NopStation.Plugin.Widgets.AnywhereSlider.Factories;
using NopStation.Plugin.Widgets.AnywhereSlider.Services;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Controllers;

public class AnywhereSliderController : NopStationPublicController
{
	private readonly ISliderModelFactory _sliderModelFactory;

	private readonly ISliderService _sliderService;

	public AnywhereSliderController(ISliderModelFactory sliderModelFactory, ISliderService sliderService)
	{
		_sliderModelFactory = sliderModelFactory;
		_sliderService = sliderService;
	}

	[HttpPost]
	public async Task<IActionResult> Details(int sliderId)
	{
		Slider slider = await _sliderService.GetSliderByIdAsync(sliderId);
		if (slider == null || slider.Deleted || !slider.Active)
		{
			return Json(new
			{
				result = false
			});
		}
		return Json(new
		{
			result = true,
			html = await RenderPartialViewToStringAsync("Details", await _sliderModelFactory.PrepareSliderModelAsync(slider)),
			sliderId = sliderId
		});
	}
}
