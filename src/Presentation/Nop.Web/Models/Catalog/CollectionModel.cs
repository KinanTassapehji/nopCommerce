using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Catalog;

/// <summary>
/// Represents one of the home page bands as a page: "deals", "bestsellers" or "featured"
/// </summary>
public partial record CollectionModel : BaseNopModel
{
    #region Properties

    /// <summary>
    /// Gets or sets which band: deals, bestsellers or featured
    /// </summary>
    public string Collection { get; set; }

    /// <summary>
    /// Gets or sets the catalog products model
    /// </summary>
    public CatalogProductsModel CatalogProductsModel { get; set; }

    #endregion

    #region Ctor

    public CollectionModel()
    {
        CatalogProductsModel = new CatalogProductsModel();
    }

    #endregion
}