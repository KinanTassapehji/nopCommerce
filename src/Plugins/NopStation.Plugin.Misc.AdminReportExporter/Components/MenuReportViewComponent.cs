using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Misc.AdminReportExporter.Components;

public class MenuReportViewComponent : NopStationViewComponent
{
	private readonly AdminReportExporterSettings _adminReportExporterSettings;

	public MenuReportViewComponent(AdminReportExporterSettings adminReportExporterSettings)
	{
		_adminReportExporterSettings = adminReportExporterSettings;
	}

	public IViewComponentResult Invoke(string widgetZone, object additionalData)
	{
		if (!_adminReportExporterSettings.EnablePlugin)
		{
			return Content("");
		}
		if (base.HttpContext.Request.RouteValues["controller"] as string != "Report")
		{
			return Content("");
		}
		return View();
	}
}
