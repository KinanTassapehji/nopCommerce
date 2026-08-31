using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.Misc.Core.Services;

public interface IProductAttributeParserApi
{
	Task<IList<ProductAttributeMapping>> ParseProductAttributeMappingsAsync(string attributesXml);

	Task<IList<ProductAttributeValue>> ParseProductAttributeValuesAsync(string attributesXml, int productAttributeMappingId = 0);

	IList<string> ParseValues(string attributesXml, int productAttributeMappingId);

	string AddProductAttribute(string attributesXml, ProductAttributeMapping productAttributeMapping, string value, int? quantity = null);

	string RemoveProductAttribute(string attributesXml, ProductAttributeMapping productAttributeMapping);

	Task<bool> AreProductAttributesEqualAsync(string attributesXml1, string attributesXml2, bool ignoreNonCombinableAttributes, bool ignoreQuantity = true);

	Task<bool?> IsConditionMetAsync(ProductAttributeMapping pam, string selectedAttributesXml);

	Task<ProductAttributeCombination> FindProductAttributeCombinationAsync(Product product, string attributesXml, bool ignoreNonCombinableAttributes = true);

	Task<IList<string>> GenerateAllCombinationsAsync(Product product, bool ignoreNonCombinableAttributes = false, IList<int> allowedAttributeIds = null);

	Task<decimal> ParseCustomerEnteredPriceAsync(Product product, IFormCollection form);

	int ParseEnteredQuantity(Product product, IFormCollection form);

	void ParseRentalDates(Product product, NameValueCollection form, out DateTime? startDate, out DateTime? endDate);

	Task<string> ParseProductAttributesAsync(Product product, NameValueCollection form, List<string> errors);

	string AddGiftCardAttribute(string attributesXml, string recipientName, string recipientEmail, string senderName, string senderEmail, string giftCardMessage);

	void GetGiftCardAttribute(string attributesXml, out string recipientName, out string recipientEmail, out string senderName, out string senderEmail, out string giftCardMessage);
}
