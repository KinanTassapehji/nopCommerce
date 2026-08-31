using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;

public record CoreLocaleResourceSearchModel : BaseSearchModel
{
	[NopResourceDisplayName("Admin.NopStation.Core.Resources.List.SearchLanguageId")]
	public int SearchLanguageId { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Resources.List.SearchResourceName")]
	public string SearchResourceName { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.Resources.List.SearchPluginSystemName")]
	public string SearchPluginSystemName { get; set; }

	public IList<SelectListItem> AvailableLanguages { get; set; }

	public IList<SelectListItem> AvailablePlugins { get; set; }

	public CoreLocaleResourceSearchModel()
	{
		AvailableLanguages = new List<SelectListItem>();
		AvailablePlugins = new List<SelectListItem>();
	}
}
