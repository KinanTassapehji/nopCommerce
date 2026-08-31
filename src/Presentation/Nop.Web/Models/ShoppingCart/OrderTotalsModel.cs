using Nop.Web.Framework.Models;

namespace Nop.Web.Models.ShoppingCart;

public partial record OrderTotalsModel : BaseNopModel
{
    public OrderTotalsModel()
    {
        TaxRates = new List<TaxRate>();
    }
    public bool IsEditable { get; set; }

    public string SubTotal { get; set; }

    public string SubTotalDiscount { get; set; }

    public string Shipping { get; set; }
    public bool RequiresShipping { get; set; }
    public string SelectedShippingMethod { get; set; }
    public bool HideShippingTotal { get; set; }

    public string PaymentMethodAdditionalFee { get; set; }

    public string Tax { get; set; }
    public IList<TaxRate> TaxRates { get; set; }
    public bool DisplayTax { get; set; }
    public bool DisplayTaxRates { get; set; }

    public string OrderTotalDiscount { get; set; }


    public string OrderTotal { get; set; }

    #region Nested classes

    public partial record TaxRate : BaseNopModel
    {
        public string Rate { get; set; }
        public string Value { get; set; }
    }

    #endregion
}