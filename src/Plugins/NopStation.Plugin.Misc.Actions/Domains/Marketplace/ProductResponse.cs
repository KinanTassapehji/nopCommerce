using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NopStation.Plugin.Misc.Core.Domains.Marketplace;

public class ProductResponse
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("shortDescription")]
	public string ShortDescription { get; set; } = string.Empty;

	[JsonPropertyName("supportedVersions")]
	public List<string> SupportedVersions { get; set; } = new List<string>();

	[JsonPropertyName("pictureUrl")]
	public string PictureUrl { get; set; } = string.Empty;

	[JsonPropertyName("price")]
	public decimal Price { get; set; }

	[JsonPropertyName("oldPrice")]
	public decimal OldPrice { get; set; }

	[JsonPropertyName("systemName")]
	public string SystemName { get; set; } = string.Empty;

	[JsonPropertyName("formattedPrice")]
	public string FormattedPrice { get; set; } = string.Empty;

	[JsonPropertyName("formattedOldPrice")]
	public string FormattedOldPrice { get; set; } = string.Empty;

	[JsonPropertyName("productUrl")]
	public string ProductUrl { get; set; } = string.Empty;
}
