using System.Text.Json.Serialization;

namespace NopStation.Plugin.Misc.Core.Domains.Marketplace;

public class CategoryResponse
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("productCount")]
	public int ProductCount { get; set; }
}
