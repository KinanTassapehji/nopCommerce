using Nop.Core.Configuration;

namespace Nop.Plugin.Tax.FixedOrByCountryStateZip;

/// <summary>
/// Represents settings of the "Fixed or by country & state & zip" tax plugin
/// </summary>
public class FixedOrByCountryStateZipTaxSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the "tax calculation by country & state & zip" method is selected
    /// </summary>
    public bool CountryStateZipEnabled { get; set; }

    public bool UsePercentage2 { get; set; }

    public bool UsePercentage3 { get; set; }

    public bool RenameRate1 { get; set; }

    public bool RenameRate2 { get; set; }

    public bool RenameRate3 { get; set; }

    public string RateName1 { get; set; }
    public string RateName2 { get; set; }
    public string RateName3 { get; set; }

}