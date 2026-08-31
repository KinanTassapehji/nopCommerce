using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;

public record SmsTemplateSearchModel : BaseSearchModel
{
	[NopResourceDisplayName("Admin.NopStation.Core.SmsTemplates.List.SearchKeywords")]
	public string SearchKeywords { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsTemplates.List.SearchActiveId")]
	public int SearchActiveId { get; set; }
}
