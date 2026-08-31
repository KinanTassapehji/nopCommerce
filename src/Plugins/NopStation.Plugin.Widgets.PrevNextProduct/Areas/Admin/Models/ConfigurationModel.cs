using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
	[NopResourceDisplayName("Admin.NopStation.PrevNextProduct.Configuration.Fields.EnableLoop")]
	public bool EnableLoop { get; set; }

	public bool EnableLoop_OverrideForStore { get; set; }

	[NopResourceDisplayName("Admin.NopStation.PrevNextProduct.Configuration.Fields.WidgetZone")]
	public string WidgetZone { get; set; }

	public bool WidgetZone_OverrideForStore { get; set; }

	[NopResourceDisplayName("Admin.NopStation.PrevNextProduct.Configuration.Fields.NavigateBasedOn")]
	public int NavigateBasedOnId { get; set; }

	public bool NavigateBasedOnId_OverrideForStore { get; set; }

	[NopResourceDisplayName("Admin.NopStation.PrevNextProduct.Configuration.Fields.ProductNameMaxLength")]
	public int ProductNameMaxLength { get; set; }

	public bool ProductNameMaxLength_OverrideForStore { get; set; }

	[NopResourceDisplayName("Admin.NopStation.PrevNextProduct.Configuration.Fields.ProductThumbnailSize")]
	public int ProductThumbnailSize { get; set; }

	public bool ProductThumbnailSize_OverrideForStore { get; set; }

	public IList<SelectListItem> AvailableNavigationTypes { get; set; }

	public int ActiveStoreScopeConfiguration { get; set; }

	public ConfigurationModel()
	{
		AvailableNavigationTypes = new List<SelectListItem>();
	}
}
