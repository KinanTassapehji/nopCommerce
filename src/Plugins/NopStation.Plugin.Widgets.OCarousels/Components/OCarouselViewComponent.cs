using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.OCarousels.Domains;
using NopStation.Plugin.Widgets.OCarousels.Factories;
using NopStation.Plugin.Widgets.OCarousels.Helpers;
using NopStation.Plugin.Widgets.OCarousels.Services;

namespace NopStation.Plugin.Widgets.OCarousels.Components;

public class OCarouselViewComponent : NopStationViewComponent
{
	private readonly IStoreContext _storeContext;

	private readonly IOCarouselService _carouselService;

	private readonly IOCarouselModelFactory _carouselModelFactory;

	private readonly OCarouselSettings _carouselSettings;

	public OCarouselViewComponent(IStoreContext storeContext, IOCarouselModelFactory carouselModelFactory, IOCarouselService carouselService, OCarouselSettings carouselSettings)
	{
		_storeContext = storeContext;
		_carouselModelFactory = carouselModelFactory;
		_carouselService = carouselService;
		_carouselSettings = carouselSettings;
	}

	public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
	{
		if (!_carouselSettings.EnableOCarousel || !OCarouselHelper.TryGetWidgetZoneId(widgetZone, out var widgetZoneId))
		{
			return Content("");
		}
		IOCarouselService carouselService = _carouselService;
		List<int> widgetZoneIds = new List<int>(1) { widgetZoneId };
		List<OCarousel> list = (await carouselService.GetAllCarouselsAsync(widgetZoneIds, null, (await _storeContext.GetCurrentStoreAsync()).Id, true)).ToList();
		if (list.Count == 0)
		{
			return Content("");
		}
		return View(await _carouselModelFactory.PrepareCarouselListModelAsync(list));
	}
}
