using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Infrastructure;

public class RouteProvider : IRouteProvider
{
	public int Priority => 1;

	public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
	{
		string text = string.Empty;
		if (DataSettingsManager.IsDatabaseInstalled() && endpointRouteBuilder.ServiceProvider.GetRequiredService<LocalizationSettings>().SeoFriendlyUrlsForLanguagesEnabled)
		{
			List<Language> source = endpointRouteBuilder.ServiceProvider.GetRequiredService<ILanguageService>().GetAllLanguagesAsync().Result.ToList();
			text = "{language:lang=" + source.FirstOrDefault().UniqueSeoCode + "}/";
		}
		endpointRouteBuilder.MapControllerRoute("AnywhereSlider", text + "load_slider_details", new
		{
			controller = "AnywhereSlider",
			action = "Details"
		});
	}
}
