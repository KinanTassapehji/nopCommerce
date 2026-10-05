using Nop.Core.Configuration;

namespace Nop.Core.Domain.Common;

/// <summary>
/// The call to action band on the home page (Content management > Home page banner).
/// Title, Text and ButtonText are localized settings
/// </summary>
public partial class HomepageBannerSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the banner is shown
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the cover picture identifier; 0 keeps the plain brand colour
    /// </summary>
    public int PictureId { get; set; }

    /// <summary>
    /// Gets or sets the heading
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the line under the heading
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the button label; empty hides the button
    /// </summary>
    public string ButtonText { get; set; }

    /// <summary>
    /// Gets or sets where the button goes; empty means the all-categories page
    /// </summary>
    public string ButtonUrl { get; set; }
}