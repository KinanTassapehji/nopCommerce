using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.AIPoweredRecommendation.GoogleAI.Models;

public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugin.AIPoweredRecommendation.GoogleAI.Enabled")]
    public bool Enabled { get; set; }

    [NopResourceDisplayName("Plugin.AIPoweredRecommendation.GoogleAI.ProjectId")]
    public string ProjectId { get; set; }

    [NopResourceDisplayName("Plugin.AIPoweredRecommendation.GoogleAI.LocationId")]
    public string LocationId { get; set; }

    [NopResourceDisplayName("Plugin.AIPoweredRecommendation.GoogleAI.CatalogId")]
    public string CatalogId { get; set; }

    [NopResourceDisplayName("Plugin.AIPoweredRecommendation.GoogleAI.BranchId")]
    public string BranchId { get; set; }

    [NopResourceDisplayName("Plugin.AIPoweredRecommendation.GoogleAI.SyncAllowed")]
    public bool SyncAllowed { get; set; }

    [NopResourceDisplayName("Plugin.AIPoweredRecommendation.GoogleAI.LogRequests")]
    public bool LogRequests { get; set; }

    [NopResourceDisplayName("Plugin.AIPoweredRecommendation.GoogleAI.SearchAllowed")]
    public bool SearchAllowed { get; set; }
}
