using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NopStation.Plugin.Misc.Core.Domains.Marketplace;

public class ApiResponse
{
	[JsonPropertyName("categories")]
	public List<CategoryResponse> Categories { get; set; } = new List<CategoryResponse>();

	[JsonPropertyName("products")]
	public List<ProductResponse> Products { get; set; } = new List<ProductResponse>();

	[JsonPropertyName("totalCount")]
	public int TotalCount { get; set; }

	[JsonPropertyName("pageNumber")]
	public int PageNumber { get; set; }

	[JsonPropertyName("pageSize")]
	public int PageSize { get; set; }

	[JsonPropertyName("marketplaceLogoUrl")]
	public string MarketplaceLogoUrl { get; set; }
}
