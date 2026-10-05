using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Common;

/// <summary>
/// Represents the home page banner settings model
/// </summary>
public partial record HomepageBannerModel : BaseNopModel, ILocalizedModel<HomepageBannerLocalizedModel>
{
    [NopResourceDisplayName("Admin.ContentManagement.HomepageBanner.Fields.Enabled")]
    public bool Enabled { get; set; }

    [UIHint("Picture")]
    [NopResourceDisplayName("Admin.ContentManagement.HomepageBanner.Fields.Picture")]
    public int PictureId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageBanner.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageBanner.Fields.Text")]
    public string Text { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageBanner.Fields.ButtonText")]
    public string ButtonText { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageBanner.Fields.ButtonUrl")]
    public string ButtonUrl { get; set; }

    public IList<HomepageBannerLocalizedModel> Locales { get; set; } = new List<HomepageBannerLocalizedModel>();
}

public partial record HomepageBannerLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageBanner.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageBanner.Fields.Text")]
    public string Text { get; set; }

    [NopResourceDisplayName("Admin.ContentManagement.HomepageBanner.Fields.ButtonText")]
    public string ButtonText { get; set; }
}