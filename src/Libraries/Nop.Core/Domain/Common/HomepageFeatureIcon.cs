namespace Nop.Core.Domain.Common;

/// <summary>
/// The icons a home page trust strip item can show. Drawn in _HomepageFeatureIcon.cshtml,
/// all in the same stroke style, so any choice sits with the others.
/// </summary>
public enum HomepageFeatureIcon
{
    /// <summary>Delivery truck</summary>
    Delivery = 1,

    /// <summary>Banknote</summary>
    CashOnDelivery = 2,

    /// <summary>Shield with a check</summary>
    Genuine = 3,

    /// <summary>Headset</summary>
    Support = 4,

    /// <summary>Circular arrow</summary>
    Returns = 5,

    /// <summary>Padlock</summary>
    SecurePayment = 6,

    /// <summary>Gift box</summary>
    Gift = 7,

    /// <summary>Price tag</summary>
    Offers = 8,

    /// <summary>Clock</summary>
    Clock = 9,

    /// <summary>Star</summary>
    Star = 10,

    /// <summary>Phone</summary>
    Phone = 11,

    /// <summary>Store front</summary>
    Store = 12
}
