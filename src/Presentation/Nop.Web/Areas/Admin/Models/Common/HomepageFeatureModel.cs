using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Common;

/// <summary>
/// Represents a home page feature model
/// </summary>
public partial record HomepageFeatureModel : BaseNopEntityModel, ILocalizedModel<HomepageFeatureLocalizedModel>
{
    #region Ctor

    public HomepageFeatureModel()
    {
        Locales = new List<HomepageFeatureLocalizedModel>();
        AvailableIcons = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.ContentManagement.HomepageFeatures.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageFeatures.Fields.Hint")]
    public string Hint { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageFeatures.Fields.Icon")]
    public int IconId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageFeatures.Fields.Published")]
    public bool Published { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageFeatures.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    public string IconName { get; set; }

    public IList<SelectListItem> AvailableIcons { get; set; }

    public IList<HomepageFeatureLocalizedModel> Locales { get; set; }

    #endregion
}

public partial record HomepageFeatureLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageFeatures.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageFeatures.Fields.Hint")]
    public string Hint { get; set; }
}
