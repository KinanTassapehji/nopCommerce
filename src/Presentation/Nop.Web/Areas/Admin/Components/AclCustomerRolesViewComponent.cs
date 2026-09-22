using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Components;

/// <summary>
/// Represents view component to display field to select customer roles
/// </summary>
public partial class AclCustomerRolesViewComponent : NopViewComponent
{
    #region Fields

    protected readonly CatalogSettings _catalogSettings;

    #endregion

    #region Ctor

    public AclCustomerRolesViewComponent(CatalogSettings catalogSettings)
    {
        _catalogSettings = catalogSettings;
    }

    #endregion

    #region Methods

    public IViewComponentResult Invoke(object additionalData)
    {
        //TmTm: hide "Limited to customer roles" while ACL is ignored, the field would do nothing
        // ponytail: reads the current store's value only; check every store (as AclDisabledWarningViewComponent does) if a second store is ever added
        if (additionalData is not IAclSupportedModel model || _catalogSettings.IgnoreAcl)
            return Content(string.Empty);
        
        return View(model);
    }

    #endregion
}