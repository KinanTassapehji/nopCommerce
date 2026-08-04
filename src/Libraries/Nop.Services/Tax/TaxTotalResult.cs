namespace Nop.Services.Tax;

/// <summary>
/// Represents a result of tax total calculation
/// </summary>
public partial class TaxTotalResult : BaseNopResult
{
    #region Ctor

    public TaxTotalResult()
    {
        TaxRates = new TaxRateResult();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Change tax total amount and recalculate tax rate results based on the new tax total amount
    /// </summary>
    /// <param name="newTaxTotal">New tax total amount</param>
    public void ChangeTaxTotal(decimal newTaxTotal)
    {
        if (TaxRates.TotalTaxRate == decimal.Zero)
            return;

        var taxAmountPart = newTaxTotal / TaxRates.TotalTaxRate;

        foreach (var definition in TaxRates.TaxDefinitions)
            definition.TaxAmount = taxAmountPart * definition.TaxRate;
    }

    /// <summary>
    /// Creates a copy of this instance
    /// </summary>
    /// <returns>Copy of tax total result</returns>
    public TaxTotalResult Copy()
    {
        return new TaxTotalResult
        {
            TaxRates = new TaxRateResult { TaxDefinitions = TaxRates.TaxDefinitions.Select(d => d.Copy()).ToList() }
        };
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets tax rates
    /// </summary>
    public TaxRateResult TaxRates { get; set; }

    #endregion
}