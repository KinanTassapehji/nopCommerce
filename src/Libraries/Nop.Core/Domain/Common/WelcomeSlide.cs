using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Common;

/// <summary>
/// Represents one slide of the welcome tour a visitor sees on the first visit (Content management > Welcome slides)
/// </summary>
public partial class WelcomeSlide : BaseEntity, ILocalizedEntity
{
    /// <summary>
    /// Gets or sets the title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the text under the title
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the picture identifier (0 = none; the icon is drawn instead)
    /// </summary>
    public int PictureId { get; set; }

    /// <summary>
    /// Gets or sets the icon identifier, drawn when there is no picture
    /// </summary>
    public int IconId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the slide is shown
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