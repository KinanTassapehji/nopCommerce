namespace Nop.Services.Tax;

/// <summary>
/// Represents a definition of a tax rate
/// </summary>
public partial class TaxDefinition
{
    #region Fields

    protected string _code;
    protected decimal _rate;
    protected decimal _amount;

    #endregion

    #region Methods

    /// <summary>
    /// Create deep copy of this instance
    /// </summary>
    /// <returns>Copy of this instance</returns>
    public TaxDefinition Copy()
    {
        return new TaxDefinition { _code = _code, _rate = _rate, _amount = _amount };
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the tax definition code. 
    /// Leave empty to use the tax rate value as a code
    /// </summary>
    /// <remarks>
    /// For example: GST, PST
    /// </remarks>
    public string Code
    {
        get
        {
            if (!string.IsNullOrEmpty(_code))
                return _code;

            return string.Empty;
        }
        set => _code = value;
    }

    /// <summary>
    /// Gets or sets the tax rate percentage
    /// </summary>
    /// <remarks>
    /// The tax rate should be a positive  or zero. 
    /// If you pass the negative value it will be set to zero
    /// </remarks>
    public decimal TaxRate
    {
        get => _rate;
        set => _rate = value >= decimal.Zero ? value : decimal.Zero;
    }

    /// <summary>
    /// Gets or sets the tax amount
    /// </summary>
    /// <remarks>
    /// The tax amount should be a positive  or zero. 
    /// If you pass the negative value it will be set to zero
    /// </remarks>
    public decimal TaxAmount
    {
        get => _amount;
        set => _amount = value >= decimal.Zero ? value : decimal.Zero;
    }

    #endregion
}