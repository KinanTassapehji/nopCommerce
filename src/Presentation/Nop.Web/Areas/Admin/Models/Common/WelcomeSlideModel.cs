using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Common;

/// <summary>
/// Represents a welcome slide model
/// </summary>
public partial record WelcomeSlideModel : BaseNopEntityModel, ILocalizedModel<WelcomeSlideLocalizedModel>
{
    #region Ctor

    public WelcomeSlideModel()
    {
        Locales = new List<WelcomeSlideLocalizedModel>();
        AvailableIcons = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.ContentManagement.WelcomeSlides.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.WelcomeSlides.Fields.Text")]
    public string Text { get; set; }

    [UIHint("Picture")]
    [NopResourceDisplayName("Admin.ContentManagement.WelcomeSlides.Fields.Picture")]
    public int PictureId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.WelcomeSlides.Fields.Icon")]
    public int IconId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.WelcomeSlides.Fields.Published")]
    public bool Published { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.WelcomeSlides.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    public string IconName { get; set; }

    public IList<SelectListItem> AvailableIcons { get; set; }

    public IList<WelcomeSlideLocalizedModel> Locales { get; set; }

    #endregion
}

public partial record WelcomeSlideLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.WelcomeSlides.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.WelcomeSlides.Fields.Text")]
    public string Text { get; set; }
}