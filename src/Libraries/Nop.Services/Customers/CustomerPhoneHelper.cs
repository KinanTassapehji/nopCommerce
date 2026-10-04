using PhoneNumbers;

namespace Nop.Services.Customers;

/// <summary>
/// The customer's phone number is their login (Customer.Username), so every phone that is
/// stored or looked up goes through here and ends up in one shape: E.164, e.g. +963933123456
/// </summary>
public static partial class CustomerPhoneHelper
{
    /// <summary>
    /// Region a number is read in when it is typed without its country code (0933 123 456)
    /// </summary>
    public const string DefaultRegion = "SY";

    /// <summary>
    /// Get the number in E.164 form
    /// </summary>
    /// <param name="phone">Number as typed: local (0933 123 456) or international (+963 933 123 456, 00963...)</param>
    /// <param name="region">Two-letter ISO code of the country picked beside the number; used only when the number has no country code</param>
    /// <returns>E.164 number; null when it is not a valid phone number</returns>
    public static string ToE164(string phone, string region)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var util = PhoneNumberUtil.GetInstance();
        try
        {
            var number = util.Parse(phone, string.IsNullOrEmpty(region) ? DefaultRegion : region.ToUpperInvariant());

            return util.IsValidNumber(number) ? util.Format(number, PhoneNumberFormat.E164) : null;
        }
        catch (NumberParseException)
        {
            return null;
        }
    }

    /// <summary>
    /// Split a stored number back into what the phone field shows: its country and the local number
    /// </summary>
    /// <param name="phone">Stored number; numbers saved before E.164 are read in the default region</param>
    /// <returns>Two-letter ISO code of the country and the number without its country code</returns>
    public static (string Region, string National) Split(string phone)
    {
        var util = PhoneNumberUtil.GetInstance();
        try
        {
            var number = util.Parse(phone ?? string.Empty, DefaultRegion);
            if (util.IsValidNumber(number))
                return (util.GetRegionCodeForNumber(number), util.Format(number, PhoneNumberFormat.NATIONAL));
        }
        catch (NumberParseException)
        {
        }

        return (DefaultRegion, phone);
    }

    /// <summary>
    /// Format a stored number for reading: +963 944 555 123
    /// </summary>
    /// <param name="phone">Number, E.164</param>
    /// <returns>Formatted number; the number as given when it does not parse</returns>
    public static string FormatInternational(string phone)
    {
        var util = PhoneNumberUtil.GetInstance();
        try
        {
            return util.Format(util.Parse(phone, DefaultRegion), PhoneNumberFormat.INTERNATIONAL);
        }
        catch (NumberParseException)
        {
            return phone;
        }
    }

    /// <summary>
    /// Get the dialling code of a country (963 for SY)
    /// </summary>
    /// <param name="region">Two-letter ISO code</param>
    /// <returns>Dialling code; 0 for a region without one</returns>
    public static int GetCountryCode(string region)
    {
        return PhoneNumberUtil.GetInstance().GetCountryCodeForRegion(region?.ToUpperInvariant());
    }
}