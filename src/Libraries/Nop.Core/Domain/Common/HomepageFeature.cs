using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Common;

/// <summary>
/// Represents one item of the home page trust strip ("Fast delivery", "Cash on delivery", ...)
/// </summary>
public partial class HomepageFeature : BaseEntity, ILocalizedEntity
{
    /// <summary>
    /// Gets or sets the title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the line under the title
    /// </summary>
    public string Hint { get; set; }

    /// <summary>
    /// Gets or sets the icon identifier
    /// </summary>
    public int IconId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is shown
    /// </summary>
    public bool Published { get; set; }

    /// <summary>
    /// Gets or sets the display order
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the icon
    /// </summary>
    public HomepageFeatureIcon Icon
    {
        get => (HomepageFeatureIcon)IconId;
        set => IconId = (int)value;
    }
}
