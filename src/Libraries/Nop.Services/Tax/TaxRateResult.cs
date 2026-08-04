using System.Globalization;
using System.Text;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;

namespace Nop.Services.Tax;

/// <summary>
/// Represents a result of tax rate calculation
/// </summary>
public partial class TaxRateResult : BaseNopResult
{
    #region Fields

    /// <summary>
    /// The separator for tax rates
    /// </summary>
    private const string SEPARATOR = ";";

    #endregion

    #region Methods

    /// <summary>
    /// Parse tax rates
    /// </summary>
    /// <param name="taxRatesStr"></param>
    /// <returns>Rates</returns>
    public static SortedDictionary<string, decimal?> ParseTaxRates(string taxRatesStr)
    {
        var taxRatesDictionary = new SortedDictionary<string, decimal?>();

        if (string.IsNullOrEmpty(taxRatesStr))
            return taxRatesDictionary;

        var lines = taxRatesStr.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var taxes = line.Trim().Split(':');

            if (taxes.Length != 2)
            {
                if (taxes.Length == 1 && taxes[0].Trim().Contains(" "))
                    taxRatesDictionary.Add(taxes[0].Trim(), null);
            }

            try
            {
                var taxRate = taxes[0].Trim();
                var taxValue = decimal.Parse(taxes[1].Trim(), CultureInfo.InvariantCulture);
                taxRatesDictionary.Add(taxRate, taxValue);
            }
            catch
            {
                // ignored
            }
        }

        //add at least one tax rate (0%)
        if (!taxRatesDictionary.Any())
            taxRatesDictionary.Add(decimal.Zero.ToString("G29", CultureInfo.InvariantCulture), decimal.Zero);

        return taxRatesDictionary;
    }

    /// <summary>
    /// Add tax rate results into the current tax rate result
    /// </summary>
    /// <param name="taxRateResult">Tax rate results</param>
    /// <returns>Concatenated tax result</returns>
    public virtual void AppendTaxResults(TaxRateResult taxRateResult)
    {
        var totalDictionary = TaxDefinitions.GroupBy(getKey, d => d).ToDictionary(d => d.Key, d => d.First());

        foreach (var definition in taxRateResult.TaxDefinitions)
        {
            var key = getKey(definition);

            if (!totalDictionary.TryGetValue(key, out var item))
                totalDictionary[key] = definition.Copy();
            else
                item.TaxAmount += definition.TaxAmount;
        }

        TaxDefinitions = totalDictionary.Values.ToList();

        return;

        static string getKey(TaxDefinition definition)
        {
            return $"{definition.Code} {definition.TaxRate}";
        }
    }

    /// <summary>
    /// Formats the tax result for display
    /// </summary>
    /// <param name="addAdditionalData">The value indicating whether to add additional data to display on the invoice and shopping cart</param>
    /// <returns>Formated tax result for display</returns>
    public virtual string FormatTaxResult(bool addAdditionalData = false)
    {
        var builder = new StringBuilder();

        if (addAdditionalData && !string.IsNullOrEmpty(TaxAdditionalData))
            builder.Append(TaxAdditionalData);

        foreach (var taxDefinition in TaxDefinitions.OrderBy(td => td.TaxRate))
        {
            var code = string.IsNullOrEmpty(taxDefinition.Code) ? string.Empty : $"{taxDefinition.Code} ";
            builder.Append($"{code}{taxDefinition.TaxRate.ToString("G29", CultureInfo.InvariantCulture)}:{taxDefinition.TaxAmount}{SEPARATOR}");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Add tax amount to the tax rate result based on tax definition
    /// </summary>
    /// <param name="taxDefinition">Tax definition</param>
    /// <param name="amount">Tax ammount to add</param>
    public virtual void AddTaxAmount(TaxDefinition taxDefinition, decimal amount)
    {
        if (amount <= decimal.Zero || taxDefinition.TaxRate <= decimal.Zero)
            return;

        var definition = TaxDefinitions.FirstOrDefault(d => d.Code.Equals(taxDefinition.Code) && d.TaxRate.Equals(taxDefinition.TaxRate));

        if (definition == null)
        {
            definition = new TaxDefinition
            {
                Code = taxDefinition.Code,
                TaxRate = taxDefinition.TaxRate,
                TaxAmount = amount
            };

            TaxDefinitions.Add(definition);
        }
        else
        {
            definition.TaxAmount += amount;
        }
    }

    /// <summary>
    /// Add tax rate with amount to the tax rate result
    /// </summary>
    /// <param name="taxRate">Tax rate</param>
    /// <param name="taxAmount">Tax amount to add</param>
    public virtual void AddTaxWithAmount(TaxRateResult taxRate, decimal taxAmount)
    {
        if (taxRate.TotalTaxRate <= decimal.Zero || taxAmount <= decimal.Zero)
            return;

        var amountPart = taxAmount / taxRate.TotalTaxRate;

        
        foreach (var definition in taxRate.TaxDefinitions)
        {
            var taxValue = definition.TaxRate * amountPart;
            AddTaxAmount(definition, taxValue);
        }
    }

    /// <summary>
    /// Add tax rate to the tax rate result
    /// </summary>
    /// <param name="taxRate">Tax rate</param>
    /// <param name="code">Tax rate code</param>
    public virtual void AddTaxRate(decimal taxRate, string code = "")
    {
        var definition = TaxDefinitions.FirstOrDefault(d => d.Code.Equals(code) && d.TaxRate.Equals(taxRate));

        if (definition != null)
            return;

        definition = new TaxDefinition
        {
            Code = code,
            TaxRate = taxRate,
            TaxAmount = decimal.Zero
        };

        TaxDefinitions.Add(definition);
    }

    /// <summary>
    /// Add tax amount to the tax rate result
    /// </summary>
    /// <param name="taxRate">Tax rate</param>
    /// <param name="taxAmount">Tax amount</param>
    public virtual TaxRateResult AddTaxAmount(decimal taxRate, decimal taxAmount)
    {
        if (taxAmount <= decimal.Zero || taxRate <= decimal.Zero)
            return this;

        var definition = TaxDefinitions.FirstOrDefault(d => d.TaxRate.Equals(taxRate));

        if (definition == null)
        {
            definition = new TaxDefinition
            {
                Code = string.Empty,
                TaxRate = taxRate,
                TaxAmount = taxAmount
            };

            TaxDefinitions.Add(definition);
        }
        else
        {
            definition.TaxAmount += taxAmount;
        }

        return this;
    }

    /// <summary>
    /// Add tax amount to the total tax rate result
    /// </summary>
    /// <param name="taxAmount">Tax amount</param>
    public virtual TaxRateResult AddTaxAmount(decimal taxAmount)
    {
        if (taxAmount <= decimal.Zero || TotalTaxRate <= decimal.Zero)
            return this;

        var taxPart = taxAmount / TotalTaxRate;

        foreach (var definition in TaxDefinitions)
            definition.TaxAmount += taxPart * definition.TaxRate;

        return this;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the tax additional data to display on the invoice and shopping cart.
    /// Leave empty if no additional data is needed
    /// </summary>
    /// <remarks>
    /// For example: HSN (goods) or SAC (services) code for the India GST
    /// </remarks>
    public string TaxAdditionalData { get; set; }

    /// <summary>
    /// Gets or sets a list of tax definitions associated with the tax rate result
    /// </summary>
    public List<TaxDefinition> TaxDefinitions { get; set; } = new();

    /// <summary>
    /// Gets the total tax rate of the tax rate result
    /// </summary>
    public decimal TotalTaxRate => TaxDefinitions.Sum(d => d.TaxRate);

    /// <summary>
    /// Gets the total tax amount of the tax rate result
    /// </summary>
    public decimal TotalTaxAmount => TaxDefinitions.Sum(d => d.TaxAmount);

    #endregion
}