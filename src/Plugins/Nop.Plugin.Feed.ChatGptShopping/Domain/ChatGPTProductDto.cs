using System.Text.Json.Serialization;

namespace Nop.Plugin.Feed.ChatGptShopping.Domain;

public class ChatGPTProductDto
{
    #region OpenAI Flags

    /// <summary>
    /// Controls whether the product can be surfaced in ChatGPT search results
    /// </summary>
    [JsonPropertyName("is_eligible_search")]
    public bool IsEligibleSearch { get; set; }

    /// <summary>
    /// Allows direct purchase inside ChatGPT. is_eligible_search must be true for is_eligible_checkout to be enabled for the product
    /// </summary>
    [JsonPropertyName("is_eligible_checkout")]
    public bool IsEligibleCheckout { get; set; }

    /// <summary>
    /// Controls whether the product can be processed for ChatGPT ads. Use is_eligible_ads only as a legacy alias
    /// </summary>
    [JsonPropertyName("is_ads_eligible")]
    public bool IsAdsEligible { get; set; }

    #endregion

    #region Basic Product Data

    /// <summary>
    /// Merchant product ID (unique per variant)
    /// </summary>
    [JsonPropertyName("item_id")]
    public string ItemId { get; set; }

    /// <summary>
    /// Universal product identifier
    /// </summary>
    [JsonPropertyName("gtin")]
    public string Gtin { get; set; }

    /// <summary>
    /// Manufacturer part number
    /// </summary>
    [JsonPropertyName("mpn")]
    public string Mpn { get; set; }

    /// <summary>
    /// Product title
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// Full product description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// Product detail page URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; }

    #endregion

    #region Item Information

    /// <summary>
    /// Product brand
    /// </summary>
    [JsonPropertyName("brand")]
    public string Brand { get; set; }

    /// <summary>
    /// Condition of product
    /// </summary>
    /// <example>new</example>
    [JsonPropertyName("condition")]
    public string Condition { get; set; }

    /// <summary>
    /// Category path
    /// </summary>
    [JsonPropertyName("product_category")]
    public string ProductCategory { get; set; }

    #endregion

    #region Media

    /// <summary>
    /// Main product image URL
    /// </summary>
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; }

    /// <summary>
    /// Extra images
    /// </summary>
    [JsonPropertyName("additional_image_urls")]
    public string AdditionalImageUrls { get; set; }

    /// <summary>
    /// Product video
    /// </summary>
    [JsonPropertyName("video_url")]
    public string VideoUrl { get; set; }

    #endregion

    #region Price & Promotions

    /// <summary>
    /// Regular price
    /// </summary>
    [JsonPropertyName("price")]
    public string Price { get; set; }

    /// <summary>
    /// Discounted price
    /// </summary>
    [JsonPropertyName("sale_price")]
    public string SalePrice { get; set; }

    #endregion

    #region Availability & Inventory

    /// <summary>
    /// Product availability
    /// </summary>
    [JsonPropertyName("availability")]
    public string Availability { get; set; }

    /// <summary>
    /// Availability date if pre-order
    /// </summary>
    [JsonPropertyName("availability_date")]
    public DateTime? AvailabilityDate { get; set; }

    #endregion

    #region Merchant Info

    /// <summary>
    /// Seller name
    /// </summary>
    [JsonPropertyName("seller_name")]
    public string SellerName { get; set; }

    /// <summary>
    /// Seller page
    /// </summary>
    [JsonPropertyName("seller_url")]
    public string SellerUrl { get; set; }

    #endregion

    #region Returns

    /// <summary>
    /// Return policy URL
    /// </summary>
    [JsonPropertyName("return_policy")]
    public string ReturnPolicy { get; set; }

    #endregion

    #region Reviews and Q&A

    /// <summary>
    /// Number of product reviews
    /// </summary>
    [JsonPropertyName("review_count")]
    public int ReviewCount { get; set; }

    /// <summary>
    /// Average review score
    /// </summary>
    [JsonPropertyName("star_rating")]
    public string StarRating { get; set; }

    #endregion

    #region Geo Tagging

    /// <summary>
    /// Target countries of the item (first entry used)
    /// </summary>
    [JsonPropertyName("target_countries")]
    public string TargetCountries { get; set; }

    /// <summary>
    /// Store country of the item
    /// </summary>
    [JsonPropertyName("store_country")]
    public string StoreCountry { get; set; }

    #endregion
}
