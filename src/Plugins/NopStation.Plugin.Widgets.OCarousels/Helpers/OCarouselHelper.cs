using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinqToDB.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Widgets.OCarousels.Helpers;

public class OCarouselHelper
{
	public static List<string> GetCustomWidgetZones()
	{
		List<string> list = new List<string>();
		try
		{
			List<WidgetZoneModel> customWidgetZoneNameValues = GetCustomWidgetZoneNameValues();
			if (!customWidgetZoneNameValues.IsNullOrEmpty())
			{
				list.AddRange((from x in customWidgetZoneNameValues
					select x.Name into s
					where !string.IsNullOrWhiteSpace(s)
					select s).Distinct());
			}
		}
		catch (Exception ex)
		{
			NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex).Wait();
		}
		return list;
	}

	public static List<WidgetZoneModel> GetCustomWidgetZoneNameValues()
	{
		List<WidgetZoneModel> result = new List<WidgetZoneModel>();
		try
		{
			INopFileProvider nopFileProvider = NopInstance.Load<INopFileProvider>();
			string text = nopFileProvider.Combine(nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Widgets.OCarousels/"), "widgetZones.json");
			if (nopFileProvider.FileExists(text))
			{
				List<WidgetZoneModel> list = JsonConvert.DeserializeObject<List<WidgetZoneModel>>(nopFileProvider.ReadAllText(text, Encoding.UTF8));
				if (!list.IsNullOrEmpty())
				{
					return list;
				}
			}
		}
		catch (Exception ex)
		{
			NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex).Wait();
		}
		return result;
	}

	public static bool TryGetWidgetZoneId(string widgetZone, out int widgetZoneId)
	{
		widgetZoneId = -1;
		List<WidgetZoneModel> customWidgetZoneNameValues = GetCustomWidgetZoneNameValues();
		if (customWidgetZoneNameValues != null && customWidgetZoneNameValues.Any((WidgetZoneModel x) => x.Name.Equals(widgetZone)))
		{
			widgetZoneId = customWidgetZoneNameValues.FirstOrDefault((WidgetZoneModel x) => x.Name.Equals(widgetZone)).Value;
			return true;
		}
		return false;
	}

	public static string GetCustomWidgetZone(int widgetZoneId)
	{
		List<WidgetZoneModel> customWidgetZoneNameValues = GetCustomWidgetZoneNameValues();
		if (customWidgetZoneNameValues != null && customWidgetZoneNameValues.Any((WidgetZoneModel x) => x.Value == widgetZoneId))
		{
			return customWidgetZoneNameValues.FirstOrDefault((WidgetZoneModel x) => x.Value == widgetZoneId)?.Name;
		}
		return null;
	}

	public static IList<SelectListItem> GetCustomWidgetZoneSelectList()
	{
		List<SelectListItem> list = new List<SelectListItem>();
		List<WidgetZoneModel> customWidgetZoneNameValues = GetCustomWidgetZoneNameValues();
		if (customWidgetZoneNameValues.IsNullOrEmpty())
		{
			return list;
		}
		list.AddRange(customWidgetZoneNameValues.Select((WidgetZoneModel item) => new SelectListItem
		{
			Value = item.Value.ToString(),
			Text = item.Name
		}));
		return list;
	}
}
