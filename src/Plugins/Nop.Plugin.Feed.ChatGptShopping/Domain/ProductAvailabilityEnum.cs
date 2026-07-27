namespace Nop.Plugin.Feed.ChatGptShopping.Domain;

/// <summary>
/// Represents product availability enumeration
/// </summary>
public enum ProductAvailabilityEnum
{
    in_stock,
    out_of_stock,
    pre_order,
    backorder,
    unknown
}
