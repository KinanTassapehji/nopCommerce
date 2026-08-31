using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Widgets.ProductTabs.Helpers;

public class ProductTabHelper
{
	public static List<string> GetCustomWidgetZones()
	{
		List<string> list = new List<string>();
		try
		{
			List<ProductTabWidgetZoneModel> customWidgetZoneNameValues = GetCustomWidgetZoneNameValues();
			if (customWidgetZoneNameValues != null && customWidgetZoneNameValues.Any())
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

	public static List<ProductTabWidgetZoneModel> GetCustomWidgetZoneNameValues()
	{
		List<ProductTabWidgetZoneModel> result = new List<ProductTabWidgetZoneModel>();
		try
		{
			INopFileProvider nopFileProvider = NopInstance.Load<INopFileProvider>();
			string text = nopFileProvider.Combine(nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Widgets.ProductTabs/"), "widgetZones.json");
			if (nopFileProvider.FileExists(text))
			{
				List<ProductTabWidgetZoneModel> list = JsonConvert.DeserializeObject<List<ProductTabWidgetZoneModel>>(nopFileProvider.ReadAllText(text, Encoding.UTF8));
				if (list != null && list.Any())
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
		List<ProductTabWidgetZoneModel> customWidgetZoneNameValues = GetCustomWidgetZoneNameValues();
		if (customWidgetZoneNameValues != null && customWidgetZoneNameValues.Any((ProductTabWidgetZoneModel x) => x.Name.Equals(widgetZone)))
		{
			widgetZoneId = customWidgetZoneNameValues.FirstOrDefault((ProductTabWidgetZoneModel x) => x.Name.Equals(widgetZone)).Value;
			return true;
		}
		return false;
	}

	public static string GetCustomWidgetZone(int widgetZoneId)
	{
		List<ProductTabWidgetZoneModel> customWidgetZoneNameValues = GetCustomWidgetZoneNameValues();
		if (customWidgetZoneNameValues != null && customWidgetZoneNameValues.Any((ProductTabWidgetZoneModel x) => x.Value == widgetZoneId))
		{
			return customWidgetZoneNameValues.FirstOrDefault((ProductTabWidgetZoneModel x) => x.Value == widgetZoneId).Name;
		}
		return null;
	}

	public static IList<SelectListItem> GetCustomWidgetZoneSelectList()
	{
		List<SelectListItem> list = new List<SelectListItem>();
		List<ProductTabWidgetZoneModel> customWidgetZoneNameValues = GetCustomWidgetZoneNameValues();
		if (customWidgetZoneNameValues != null && customWidgetZoneNameValues.Any())
		{
			foreach (ProductTabWidgetZoneModel item in customWidgetZoneNameValues)
			{
				list.Add(new SelectListItem
				{
					Value = item.Value.ToString(),
					Text = item.Name
				});
			}
		}
		return list;
	}
}
