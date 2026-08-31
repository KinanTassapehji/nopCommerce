using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdminReportExporter;

public class NopModelHelper
{
	public static string PropertyLabel<T>(Expression<Func<T, object>> func)
	{
		if (!(func.Body is MemberExpression { Member: PropertyInfo member }))
		{
			return string.Empty;
		}
		return PropertyLabel<T>(member.Name);
	}

	public static string PropertyLabel<T>(string propertyName)
	{
		if (!EngineContext.Current.Resolve<IModelMetadataProvider>().GetMetadataForProperty(typeof(T), propertyName).AdditionalValues.TryGetValue("NopResourceDisplayNameAttribute", out object value) || !(value is NopResourceDisplayNameAttribute nopResourceDisplayNameAttribute) || string.IsNullOrEmpty(nopResourceDisplayNameAttribute.DisplayName))
		{
			return propertyName;
		}
		return nopResourceDisplayNameAttribute.DisplayName;
	}
}
