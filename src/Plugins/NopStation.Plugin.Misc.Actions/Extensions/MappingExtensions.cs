using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Services.Attributes;
using Nop.Services.Common;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.Core.Extensions;

public static class MappingExtensions
{
	public static async Task<string> ParseCustomAddressAttributesAsync(this NameValueCollection form, IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser, IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService)
	{
		if (form == null)
		{
			return null;
		}
		string attributesXml = string.Empty;
		foreach (AddressAttribute attribute in await addressAttributeService.GetAllAttributesAsync())
		{
			string name = string.Format(NopCommonDefaults.AddressAttributeControlName, attribute.Id);
			switch (attribute.AttributeControlType)
			{
			case AttributeControlType.DropdownList:
			case AttributeControlType.RadioList:
			{
				string text2 = form[name];
				if (!string.IsNullOrEmpty(text2))
				{
					int num = int.Parse(text2);
					if (num > 0)
					{
						attributesXml = addressAttributeParser.AddAttribute(attributesXml, attribute, num.ToString());
					}
				}
				break;
			}
			case AttributeControlType.Checkboxes:
			{
				string text3 = form[name];
				if (string.IsNullOrEmpty(text3))
				{
					break;
				}
				string[] array = text3.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					int num3 = int.Parse(array[num2]);
					if (num3 > 0)
					{
						attributesXml = addressAttributeParser.AddAttribute(attributesXml, attribute, num3.ToString());
					}
				}
				break;
			}
			case AttributeControlType.ReadonlyCheckboxes:
				foreach (int item in (from v in await addressAttributeService.GetAttributeValuesAsync(attribute.Id)
					where v.IsPreSelected
					select v.Id).ToList())
				{
					attributesXml = addressAttributeParser.AddAttribute(attributesXml, attribute, item.ToString());
				}
				break;
			case AttributeControlType.TextBox:
			case AttributeControlType.MultilineTextbox:
			{
				string text = form[name];
				if (!string.IsNullOrEmpty(text))
				{
					string value = text.Trim();
					attributesXml = addressAttributeParser.AddAttribute(attributesXml, attribute, value);
				}
				break;
			}
			}
		}
		return attributesXml;
	}

	public static NameValueCollection ToNameValueCollection(this List<KeyValueApi> formValues)
	{
		NameValueCollection nameValueCollection = new NameValueCollection();
		if (formValues == null)
		{
			return nameValueCollection;
		}
		foreach (KeyValueApi formValue in formValues)
		{
			nameValueCollection.Add(formValue.Key, formValue.Value);
		}
		return nameValueCollection;
	}

	public static List<T> GetFormValues<T>(this NameValueCollection form, string name, char separator = ',', StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries)
	{
		if (form[name] != null)
		{
			return (from idString in form[name].Split(new char[1] { separator }, splitOptions)
				select (T)Convert.ChangeType(idString, typeof(T))).Distinct().ToList();
		}
		return new List<T>();
	}

	public static T GetFormValue<T>(this NameValueCollection form, string name)
	{
		if (form[name] != null)
		{
			return (T)Convert.ChangeType(form[name], typeof(T));
		}
		return default(T);
	}

	public static IList<string> GetErrors(this ModelStateDictionary modelState)
	{
		List<string> list = new List<string>();
		foreach (ModelStateEntry value in modelState.Values)
		{
			foreach (ModelError error in value.Errors)
			{
				list.Add(error.ErrorMessage);
			}
		}
		return list;
	}

	public static GenericResponseModel<T> ToGenericResponse<T>(this T data)
	{
		return new GenericResponseModel<T>
		{
			Data = data
		};
	}
}
