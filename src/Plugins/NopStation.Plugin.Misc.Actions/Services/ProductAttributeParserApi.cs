using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;

namespace NopStation.Plugin.Misc.Core.Services;

public class ProductAttributeParserApi : IProductAttributeParserApi
{
	private readonly ICurrencyService _currencyService;

	private readonly IDownloadService _downloadService;

	private readonly ILocalizationService _localizationService;

	private readonly IProductAttributeService _productAttributeService;

	private readonly IRepository<ProductAttributeValue> _productAttributeValueRepository;

	private readonly IWorkContext _workContext;

	public ProductAttributeParserApi(ICurrencyService currencyService, IDownloadService downloadService, ILocalizationService localizationService, IProductAttributeService productAttributeService, IRepository<ProductAttributeValue> productAttributeValueRepository, IWorkContext workContext)
	{
		_currencyService = currencyService;
		_downloadService = downloadService;
		_productAttributeService = productAttributeService;
		_productAttributeValueRepository = productAttributeValueRepository;
		_workContext = workContext;
		_localizationService = localizationService;
	}

	protected virtual IList<IList<T>> CreateCombination<T>(IList<T> elements)
	{
		List<IList<T>> list = new List<IList<T>>();
		for (int i = 1; (double)i < Math.Pow(2.0, elements.Count); i++)
		{
			List<T> list2 = new List<T>();
			int num = -1;
			string text = Convert.ToString(i, 2).PadLeft(elements.Count, '0');
			foreach (char num2 in text)
			{
				num++;
				if (num2 != '0')
				{
					list2.Add(elements[num]);
				}
			}
			list.Add(list2);
		}
		return list;
	}

	protected virtual IList<int> ParseProductAttributeMappingIds(string attributesXml)
	{
		List<int> list = new List<int>();
		if (string.IsNullOrEmpty(attributesXml))
		{
			return list;
		}
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(attributesXml);
			foreach (XmlNode item in xmlDocument.SelectNodes("//Attributes/ProductAttribute"))
			{
				if (item.Attributes?["ID"] != null && int.TryParse(item.Attributes["ID"].InnerText.Trim(), out var result))
				{
					list.Add(result);
				}
			}
		}
		catch (Exception)
		{
		}
		return list;
	}

	protected IList<Tuple<string, string>> ParseValuesWithQuantity(string attributesXml, int productAttributeMappingId)
	{
		List<Tuple<string, string>> list = new List<Tuple<string, string>>();
		if (string.IsNullOrEmpty(attributesXml))
		{
			return list;
		}
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(attributesXml);
			foreach (XmlNode item2 in xmlDocument.SelectNodes("//Attributes/ProductAttribute"))
			{
				if (item2.Attributes?["ID"] == null || !int.TryParse(item2.Attributes["ID"].InnerText.Trim(), out var result) || result != productAttributeMappingId)
				{
					continue;
				}
				foreach (XmlNode item3 in item2.SelectNodes("ProductAttributeValue"))
				{
					string item = item3.SelectSingleNode("Value").InnerText.Trim();
					XmlNode xmlNode2 = item3.SelectSingleNode("Quantity");
					list.Add(new Tuple<string, string>(item, (xmlNode2 != null) ? xmlNode2.InnerText.Trim() : string.Empty));
				}
			}
		}
		catch
		{
		}
		return list;
	}

	protected virtual async Task<string> GetProductAttributesXmlAsync(Product product, NameValueCollection form, List<string> errors)
	{
		string attributesXml = string.Empty;
		IList<ProductAttributeMapping> productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
		foreach (ProductAttributeMapping attribute in productAttributes)
		{
			string text = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
			switch (attribute.AttributeControlType)
			{
			case AttributeControlType.DropdownList:
			case AttributeControlType.RadioList:
			case AttributeControlType.ColorSquares:
			case AttributeControlType.ImageSquares:
			{
				string text2 = form[text];
				if (StringValues.IsNullOrEmpty(text2))
				{
					break;
				}
				int selectedAttributeId = int.Parse(text2);
				if (selectedAttributeId > 0)
				{
					int quantity = 1;
					string text3 = form[$"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
					if (!StringValues.IsNullOrEmpty(text3) && (!int.TryParse(text3, out quantity) || quantity < 1))
					{
						List<string> list = errors;
						list.Add(await _localizationService.GetResourceAsync("Products.QuantityShouldBePositive"));
					}
					attributesXml = AddProductAttribute(attributesXml, attribute, selectedAttributeId.ToString(), (quantity > 1) ? new int?(quantity) : ((int?)null));
				}
				break;
			}
			case AttributeControlType.Checkboxes:
			{
				string text5 = form[text];
				if (StringValues.IsNullOrEmpty(text5))
				{
					break;
				}
				string[] array = text5.ToString().Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string text6 in array)
				{
					int quantity = int.Parse(text6);
					if (quantity > 0)
					{
						int quantity2 = 1;
						string text7 = form[$"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{text6}_qty"];
						if (!StringValues.IsNullOrEmpty(text7) && (!int.TryParse(text7, out quantity2) || quantity2 < 1))
						{
							List<string> list = errors;
							list.Add(await _localizationService.GetResourceAsync("Products.QuantityShouldBePositive"));
						}
						attributesXml = AddProductAttribute(attributesXml, attribute, quantity.ToString(), (quantity2 > 1) ? new int?(quantity2) : ((int?)null));
					}
				}
				break;
			}
			case AttributeControlType.ReadonlyCheckboxes:
				foreach (int selectedAttributeId in (from v in await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id)
					where v.IsPreSelected
					select v.Id).ToList())
				{
					int quantity = 1;
					string text4 = form[$"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
					if (!StringValues.IsNullOrEmpty(text4) && (!int.TryParse(text4, out quantity) || quantity < 1))
					{
						List<string> list = errors;
						list.Add(await _localizationService.GetResourceAsync("Products.QuantityShouldBePositive"));
					}
					attributesXml = AddProductAttribute(attributesXml, attribute, selectedAttributeId.ToString(), (quantity > 1) ? new int?(quantity) : ((int?)null));
				}
				break;
			case AttributeControlType.TextBox:
			case AttributeControlType.MultilineTextbox:
			{
				string text8 = form[text];
				if (!StringValues.IsNullOrEmpty(text8))
				{
					string value = text8.ToString().Trim();
					attributesXml = AddProductAttribute(attributesXml, attribute, value);
				}
				break;
			}
			case AttributeControlType.Datepicker:
			{
				string s = form[text + "_day"];
				string s2 = form[text + "_month"];
				string s3 = form[text + "_year"];
				DateTime? dateTime = null;
				try
				{
					dateTime = new DateTime(int.Parse(s3), int.Parse(s2), int.Parse(s));
				}
				catch
				{
				}
				if (dateTime.HasValue)
				{
					attributesXml = AddProductAttribute(attributesXml, attribute, dateTime.Value.ToString("D"));
				}
				break;
			}
			case AttributeControlType.FileUpload:
			{
				Download download = await _downloadService.GetDownloadByGuidAsync(new Guid(form[text]));
				if (download != null)
				{
					attributesXml = AddProductAttribute(attributesXml, attribute, download.DownloadGuid.ToString());
				}
				break;
			}
			}
		}
		foreach (ProductAttributeMapping attribute in productAttributes)
		{
			bool? flag = await IsConditionMetAsync(attribute, attributesXml);
			if (flag.HasValue && !flag.Value)
			{
				attributesXml = RemoveProductAttribute(attributesXml, attribute);
			}
		}
		return attributesXml;
	}

	public virtual async Task<IList<ProductAttributeMapping>> ParseProductAttributeMappingsAsync(string attributesXml)
	{
		List<ProductAttributeMapping> result = new List<ProductAttributeMapping>();
		if (string.IsNullOrEmpty(attributesXml))
		{
			return result;
		}
		IList<int> list = ParseProductAttributeMappingIds(attributesXml);
		foreach (int item in list)
		{
			ProductAttributeMapping productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(item);
			if (productAttributeMapping != null)
			{
				result.Add(productAttributeMapping);
			}
		}
		return result;
	}

	public virtual async Task<IList<ProductAttributeValue>> ParseProductAttributeValuesAsync(string attributesXml, int productAttributeMappingId = 0)
	{
		List<ProductAttributeValue> values = new List<ProductAttributeValue>();
		if (string.IsNullOrEmpty(attributesXml))
		{
			return values;
		}
		IList<ProductAttributeMapping> list = await ParseProductAttributeMappingsAsync(attributesXml);
		if (productAttributeMappingId > 0)
		{
			list = list.Where((ProductAttributeMapping productAttributeMapping) => productAttributeMapping.Id == productAttributeMappingId).ToList();
		}
		foreach (ProductAttributeMapping attribute in list)
		{
			if (!attribute.ShouldHaveValues())
			{
				continue;
			}
			foreach (Tuple<string, string> attributeValue in ParseValuesWithQuantity(attributesXml, attribute.Id))
			{
				if (string.IsNullOrEmpty(attributeValue.Item1) || !int.TryParse(attributeValue.Item1, out var result))
				{
					continue;
				}
				ProductAttributeValue productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(result);
				if (productAttributeValue != null)
				{
					if (!string.IsNullOrEmpty(attributeValue.Item2) && int.TryParse(attributeValue.Item2, out var quantity) && quantity != productAttributeValue.Quantity)
					{
						ProductAttributeValue productAttributeValue2 = await _productAttributeValueRepository.LoadOriginalCopyAsync(productAttributeValue);
						productAttributeValue2.ProductAttributeMappingId = attribute.Id;
						productAttributeValue2.Quantity = quantity;
						values.Add(productAttributeValue2);
					}
					else
					{
						values.Add(productAttributeValue);
					}
				}
			}
		}
		return values;
	}

	public virtual IList<string> ParseValues(string attributesXml, int productAttributeMappingId)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(attributesXml))
		{
			return list;
		}
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(attributesXml);
			foreach (XmlNode item2 in xmlDocument.SelectNodes("//Attributes/ProductAttribute"))
			{
				if (item2.Attributes?["ID"] == null || !int.TryParse(item2.Attributes["ID"].InnerText.Trim(), out var result) || result != productAttributeMappingId)
				{
					continue;
				}
				foreach (XmlNode item3 in item2.SelectNodes("ProductAttributeValue/Value"))
				{
					string item = item3.InnerText.Trim();
					list.Add(item);
				}
			}
		}
		catch (Exception)
		{
		}
		return list;
	}

	public virtual string AddProductAttribute(string attributesXml, ProductAttributeMapping productAttributeMapping, string value, int? quantity = null)
	{
		string result = string.Empty;
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			if (string.IsNullOrEmpty(attributesXml))
			{
				XmlElement newChild = xmlDocument.CreateElement("Attributes");
				xmlDocument.AppendChild(newChild);
			}
			else
			{
				xmlDocument.LoadXml(attributesXml);
			}
			XmlElement xmlElement = (XmlElement)xmlDocument.SelectSingleNode("//Attributes");
			XmlElement xmlElement2 = null;
			foreach (XmlNode item in xmlDocument.SelectNodes("//Attributes/ProductAttribute"))
			{
				if (item.Attributes?["ID"] != null && int.TryParse(item.Attributes["ID"].InnerText.Trim(), out var result2) && result2 == productAttributeMapping.Id)
				{
					xmlElement2 = (XmlElement)item;
					break;
				}
			}
			if (xmlElement2 == null)
			{
				xmlElement2 = xmlDocument.CreateElement("ProductAttribute");
				xmlElement2.SetAttribute("ID", productAttributeMapping.Id.ToString());
				xmlElement.AppendChild(xmlElement2);
			}
			XmlElement xmlElement3 = xmlDocument.CreateElement("ProductAttributeValue");
			xmlElement2.AppendChild(xmlElement3);
			XmlElement xmlElement4 = xmlDocument.CreateElement("Value");
			xmlElement4.InnerText = value;
			xmlElement3.AppendChild(xmlElement4);
			if (quantity.HasValue)
			{
				XmlElement xmlElement5 = xmlDocument.CreateElement("Quantity");
				xmlElement5.InnerText = quantity.ToString();
				xmlElement3.AppendChild(xmlElement5);
			}
			result = xmlDocument.OuterXml;
		}
		catch (Exception)
		{
		}
		return result;
	}

	public virtual string RemoveProductAttribute(string attributesXml, ProductAttributeMapping productAttributeMapping)
	{
		string result = string.Empty;
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			if (string.IsNullOrEmpty(attributesXml))
			{
				XmlElement newChild = xmlDocument.CreateElement("Attributes");
				xmlDocument.AppendChild(newChild);
			}
			else
			{
				xmlDocument.LoadXml(attributesXml);
			}
			XmlElement xmlElement = (XmlElement)xmlDocument.SelectSingleNode("//Attributes");
			XmlElement xmlElement2 = null;
			foreach (XmlNode item in xmlDocument.SelectNodes("//Attributes/ProductAttribute"))
			{
				if (item.Attributes?["ID"] != null && int.TryParse(item.Attributes["ID"].InnerText.Trim(), out var result2) && result2 == productAttributeMapping.Id)
				{
					xmlElement2 = (XmlElement)item;
					break;
				}
			}
			if (xmlElement2 != null)
			{
				xmlElement.RemoveChild(xmlElement2);
			}
			result = xmlDocument.OuterXml;
		}
		catch (Exception)
		{
		}
		return result;
	}

	public virtual async Task<bool> AreProductAttributesEqualAsync(string attributesXml1, string attributesXml2, bool ignoreNonCombinableAttributes, bool ignoreQuantity = true)
	{
		IList<ProductAttributeMapping> attributes1 = await ParseProductAttributeMappingsAsync(attributesXml1);
		if (ignoreNonCombinableAttributes)
		{
			attributes1 = attributes1.Where((ProductAttributeMapping x) => !x.IsNonCombinable()).ToList();
		}
		IList<ProductAttributeMapping> list = await ParseProductAttributeMappingsAsync(attributesXml2);
		if (ignoreNonCombinableAttributes)
		{
			list = list.Where((ProductAttributeMapping x) => !x.IsNonCombinable()).ToList();
		}
		if (attributes1.Count != list.Count)
		{
			return false;
		}
		bool result = true;
		foreach (ProductAttributeMapping item in attributes1)
		{
			bool flag = false;
			foreach (ProductAttributeMapping item2 in list)
			{
				if (item.Id != item2.Id)
				{
					continue;
				}
				flag = true;
				IList<Tuple<string, string>> list2 = ParseValuesWithQuantity(attributesXml1, item.Id);
				IList<Tuple<string, string>> list3 = ParseValuesWithQuantity(attributesXml2, item2.Id);
				if (list2.Count == list3.Count)
				{
					foreach (Tuple<string, string> item3 in list2)
					{
						bool flag2 = false;
						foreach (Tuple<string, string> item4 in list3)
						{
							if (!(item3.Item1.Trim() != item4.Item1.Trim()))
							{
								flag2 = ignoreQuantity || item3.Item2.Trim() == item4.Item2.Trim();
								break;
							}
						}
						if (!flag2)
						{
							result = false;
							break;
						}
					}
					continue;
				}
				result = false;
				break;
			}
			if (!flag)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public virtual async Task<bool?> IsConditionMetAsync(ProductAttributeMapping pam, string selectedAttributesXml)
	{
		ArgumentNullException.ThrowIfNull(pam, "pam");
		string conditionAttributeXml = pam.ConditionAttributeXml;
		if (string.IsNullOrEmpty(conditionAttributeXml))
		{
			return null;
		}
		ProductAttributeMapping productAttributeMapping = (await ParseProductAttributeMappingsAsync(conditionAttributeXml)).FirstOrDefault();
		if (productAttributeMapping == null)
		{
			return true;
		}
		List<string> list = (from x in ParseValues(conditionAttributeXml, productAttributeMapping.Id)
			where !string.IsNullOrEmpty(x)
			select x).ToList();
		IList<string> list2 = ParseValues(selectedAttributesXml, productAttributeMapping.Id);
		if (list.Count != list2.Count)
		{
			return false;
		}
		bool value = true;
		foreach (string item in list)
		{
			bool flag = false;
			foreach (string item2 in list2)
			{
				if (item == item2)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				value = false;
			}
		}
		return value;
	}

	public virtual async Task<ProductAttributeCombination> FindProductAttributeCombinationAsync(Product product, string attributesXml, bool ignoreNonCombinableAttributes = true)
	{
		ArgumentNullException.ThrowIfNull(product, "product");
		if (string.IsNullOrEmpty(attributesXml))
		{
			return null;
		}
		return await (await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id)).FirstOrDefaultAwaitAsync(async (ProductAttributeCombination x) => await AreProductAttributesEqualAsync(x.AttributesXml, attributesXml, ignoreNonCombinableAttributes));
	}

	public virtual async Task<IList<string>> GenerateAllCombinationsAsync(Product product, bool ignoreNonCombinableAttributes = false, IList<int> allowedAttributeIds = null)
	{
		ArgumentNullException.ThrowIfNull(product, "product");
		IList<ProductAttributeMapping> allProductAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
		if (ignoreNonCombinableAttributes)
		{
			allProductAttributeMappings = allProductAttributeMappings.Where((ProductAttributeMapping x) => !x.IsNonCombinable()).ToList();
		}
		IList<IList<ProductAttributeMapping>> list = CreateCombination(allProductAttributeMappings);
		List<string> allAttributesXml = new List<string>();
		foreach (IList<ProductAttributeMapping> item in list)
		{
			List<string> attributesXml = new List<string>();
			foreach (ProductAttributeMapping productAttributeMapping in item)
			{
				if (!productAttributeMapping.ShouldHaveValues())
				{
					continue;
				}
				IList<ProductAttributeValue> list2 = await _productAttributeService.GetProductAttributeValuesAsync(productAttributeMapping.Id);
				if (allowedAttributeIds?.Any() ?? false)
				{
					list2 = list2.Where((ProductAttributeValue attributeValue) => allowedAttributeIds.Contains(attributeValue.Id)).ToList();
				}
				if (!list2.Any())
				{
					continue;
				}
				bool num = productAttributeMapping.AttributeControlType == AttributeControlType.Checkboxes || productAttributeMapping.AttributeControlType == AttributeControlType.ReadonlyCheckboxes;
				List<string> list3 = new List<string>();
				if (num)
				{
					foreach (string item2 in attributesXml.Any() ? attributesXml : new List<string> { string.Empty })
					{
						foreach (IList<ProductAttributeValue> item3 in CreateCombination(list2))
						{
							string text = item2;
							foreach (ProductAttributeValue item4 in item3)
							{
								text = AddProductAttribute(text, productAttributeMapping, item4.Id.ToString());
							}
							if (!string.IsNullOrEmpty(text))
							{
								list3.Add(text);
							}
						}
					}
				}
				else
				{
					foreach (string oldXml in attributesXml.Any() ? attributesXml : new List<string> { string.Empty })
					{
						list3.AddRange(list2.Select((ProductAttributeValue attributeValue) => AddProductAttribute(oldXml, productAttributeMapping, attributeValue.Id.ToString())));
					}
				}
				attributesXml.Clear();
				attributesXml.AddRange(list3);
			}
			allAttributesXml.AddRange(attributesXml);
		}
		for (int i = 0; i < allAttributesXml.Count; i++)
		{
			string attributesXml2 = allAttributesXml[i];
			foreach (ProductAttributeMapping attribute in allProductAttributeMappings)
			{
				bool? flag = await IsConditionMetAsync(attribute, attributesXml2);
				if (flag.HasValue && !flag.Value)
				{
					allAttributesXml[i] = RemoveProductAttribute(attributesXml2, attribute);
				}
			}
		}
		return allAttributesXml;
	}

	public virtual async Task<decimal> ParseCustomerEnteredPriceAsync(Product product, IFormCollection form)
	{
		ArgumentNullException.ThrowIfNull(product, "product");
		ArgumentNullException.ThrowIfNull(form, "form");
		decimal result = 0m;
		if (product.CustomerEntersPrice)
		{
			foreach (string key in form.Keys)
			{
				if (key.Equals($"addtocart_{product.Id}.CustomerEnteredPrice", StringComparison.InvariantCultureIgnoreCase))
				{
					if (decimal.TryParse(form[key], out var result2))
					{
						ICurrencyService currencyService = _currencyService;
						decimal amount = result2;
						result = await currencyService.ConvertToPrimaryStoreCurrencyAsync(amount, await _workContext.GetWorkingCurrencyAsync());
					}
					break;
				}
			}
		}
		return result;
	}

	public virtual int ParseEnteredQuantity(Product product, IFormCollection form)
	{
		ArgumentNullException.ThrowIfNull(product, "product");
		ArgumentNullException.ThrowIfNull(form, "form");
		int result = 1;
		foreach (string key in form.Keys)
		{
			if (key.Equals($"addtocart_{product.Id}.EnteredQuantity", StringComparison.InvariantCultureIgnoreCase))
			{
				if (int.TryParse(form[key], out var result2))
				{
					result = result2;
				}
				break;
			}
		}
		return result;
	}

	public virtual void ParseRentalDates(Product product, NameValueCollection form, out DateTime? startDate, out DateTime? endDate)
	{
		ArgumentNullException.ThrowIfNull(product, "product");
		ArgumentNullException.ThrowIfNull(form, "form");
		startDate = null;
		endDate = null;
		if (product.IsRental)
		{
			string name = $"rental_start_date_{product.Id}";
			string name2 = $"rental_end_date_{product.Id}";
			string s = form[name];
			string s2 = form[name2];
			try
			{
				startDate = DateTime.ParseExact(s, "d", CultureInfo.InvariantCulture);
				endDate = DateTime.ParseExact(s2, "d", CultureInfo.InvariantCulture);
			}
			catch
			{
			}
		}
	}

	public virtual async Task<string> ParseProductAttributesAsync(Product product, NameValueCollection form, List<string> errors)
	{
		ArgumentNullException.ThrowIfNull(product, "product");
		ArgumentNullException.ThrowIfNull(form, "form");
		string attributesXml = await GetProductAttributesXmlAsync(product, form, errors);
		return attributesXml;
	}

	public string AddGiftCardAttribute(string attributesXml, string recipientName, string recipientEmail, string senderName, string senderEmail, string giftCardMessage)
	{
		string result = string.Empty;
		try
		{
			recipientName = recipientName.Trim();
			recipientEmail = recipientEmail.Trim();
			senderName = senderName.Trim();
			senderEmail = senderEmail.Trim();
			XmlDocument xmlDocument = new XmlDocument();
			if (string.IsNullOrEmpty(attributesXml))
			{
				XmlElement newChild = xmlDocument.CreateElement("Attributes");
				xmlDocument.AppendChild(newChild);
			}
			else
			{
				xmlDocument.LoadXml(attributesXml);
			}
			XmlElement xmlElement = (XmlElement)xmlDocument.SelectSingleNode("//Attributes");
			XmlElement xmlElement2 = (XmlElement)xmlDocument.SelectSingleNode("//Attributes/GiftCardInfo");
			if (xmlElement2 == null)
			{
				xmlElement2 = xmlDocument.CreateElement("GiftCardInfo");
				xmlElement.AppendChild(xmlElement2);
			}
			XmlElement xmlElement3 = xmlDocument.CreateElement("RecipientName");
			xmlElement3.InnerText = recipientName;
			xmlElement2.AppendChild(xmlElement3);
			XmlElement xmlElement4 = xmlDocument.CreateElement("RecipientEmail");
			xmlElement4.InnerText = recipientEmail;
			xmlElement2.AppendChild(xmlElement4);
			XmlElement xmlElement5 = xmlDocument.CreateElement("SenderName");
			xmlElement5.InnerText = senderName;
			xmlElement2.AppendChild(xmlElement5);
			XmlElement xmlElement6 = xmlDocument.CreateElement("SenderEmail");
			xmlElement6.InnerText = senderEmail;
			xmlElement2.AppendChild(xmlElement6);
			XmlElement xmlElement7 = xmlDocument.CreateElement("Message");
			xmlElement7.InnerText = giftCardMessage;
			xmlElement2.AppendChild(xmlElement7);
			result = xmlDocument.OuterXml;
		}
		catch (Exception)
		{
		}
		return result;
	}

	public void GetGiftCardAttribute(string attributesXml, out string recipientName, out string recipientEmail, out string senderName, out string senderEmail, out string giftCardMessage)
	{
		recipientName = string.Empty;
		recipientEmail = string.Empty;
		senderName = string.Empty;
		senderEmail = string.Empty;
		giftCardMessage = string.Empty;
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(attributesXml);
			XmlElement xmlElement = (XmlElement)xmlDocument.SelectSingleNode("//Attributes/GiftCardInfo/RecipientName");
			XmlElement xmlElement2 = (XmlElement)xmlDocument.SelectSingleNode("//Attributes/GiftCardInfo/RecipientEmail");
			XmlElement xmlElement3 = (XmlElement)xmlDocument.SelectSingleNode("//Attributes/GiftCardInfo/SenderName");
			XmlElement xmlElement4 = (XmlElement)xmlDocument.SelectSingleNode("//Attributes/GiftCardInfo/SenderEmail");
			XmlElement xmlElement5 = (XmlElement)xmlDocument.SelectSingleNode("//Attributes/GiftCardInfo/Message");
			if (xmlElement != null)
			{
				recipientName = xmlElement.InnerText;
			}
			if (xmlElement2 != null)
			{
				recipientEmail = xmlElement2.InnerText;
			}
			if (xmlElement3 != null)
			{
				senderName = xmlElement3.InnerText;
			}
			if (xmlElement4 != null)
			{
				senderEmail = xmlElement4.InnerText;
			}
			if (xmlElement5 != null)
			{
				giftCardMessage = xmlElement5.InnerText;
			}
		}
		catch (Exception)
		{
		}
	}
}
