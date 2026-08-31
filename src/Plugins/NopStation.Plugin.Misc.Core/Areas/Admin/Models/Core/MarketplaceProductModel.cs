using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.Core.Domains.Marketplace;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;

public record MarketplaceProductModel : BaseNopModel
{
	public int Id { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Marketplace.Fields.Name")]
	public string Name { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Marketplace.Fields.ShortDescription")]
	public string ShortDescription { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Marketplace.Fields.SupportedVersions")]
	public IList<string> SupportedVersions { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Marketplace.Fields.PictureUrl")]
	public string PictureUrl { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Marketplace.Fields.Price")]
	public decimal Price { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Marketplace.Fields.OldPrice")]
	public decimal OldPrice { get; set; }

	public string SystemName { get; set; }

	public bool IsInstalled { get; set; }

	public ButtonAction ButtonAction { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Marketplace.Fields.FormattedPrice")]
	public string FormattedPrice { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Marketplace.Fields.FormattedOldPrice")]
	public string FormattedOldPrice { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Marketplace.Fields.ProductUrl")]
	public string ProductUrl { get; set; }

	public MarketplaceProductModel()
	{
		SupportedVersions = new List<string>();
	}
}
